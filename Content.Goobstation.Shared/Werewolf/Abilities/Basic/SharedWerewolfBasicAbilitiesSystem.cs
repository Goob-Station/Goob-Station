using System.Numerics;
using Content.Shared._White.Jump;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Werewolf.Abilities.Basic;

public sealed class SharedWerewolfBasicAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    [Dependency] private readonly ThrownItemSystem _throwingItem = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, HowlEvent>(DoHowl);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfUpgradeAbility>(OnUpgradeAbility);

        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, WerewolfAmbushActionEvent>(OnAmbush);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, ThrowDoHitEvent>(OnHit);
    }

    public void OnStartup(EntityUid uid, WerewolfBasicAbilitiesComponent comp, ref ComponentStartup args)
    {
        if (_tag.HasTag(uid, "VulpEmotes"))
        {
            comp.CurrentMutation = "WerewolfTransformWerehuman";
            return;
        }
        comp.CurrentMutation = "WerewolfTransformBasic"; // goida
    }

    # region action handlers
    private void DoHowl(EntityUid uid, WerewolfBasicAbilitiesComponent comp, ref HowlEvent args) //kill me for copying changeling system please
    {
        _audio.PlayPredicted(comp.ShriekSound, uid, uid);

        var center = Transform(uid).MapPosition;
        var gamers = Filter.Empty();
        gamers.AddInRange(center, args.ShriekPower, _player, EntityManager);

        foreach (var gamer in gamers.Recipients)
        {
            if (gamer.AttachedEntity == null)
                continue;

            var pos = Transform(gamer.AttachedEntity!.Value).WorldPosition;
            var delta = center.Position - pos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(.01f, 0);

            _recoil.KickCamera(uid, -delta.Normalized());
            foreach (var entity in _entityLookup.GetEntitiesInRange(uid, args.ShriekPower))
            {
                _stun.TryUpdateStunDuration(entity, TimeSpan.FromSeconds(args.StunDuration));
                _stun.TryKnockdown(entity, TimeSpan.FromSeconds(args.StunDuration), true);
            }
        }
        // _audio.PlayGlobal(comp.DistantSound, uid); // when you howl, everyone on the station hears a quiet distant howl, which breaks the metashield for the chaplain, "allegedly" todo uncomment when better sound is found
        args.Handled = true;
    }
    private void OnAmbush(EntityUid uid, WerewolfBasicAbilitiesComponent comp, WerewolfAmbushActionEvent args) // partially taken from xenos jump
    {
        if (args.Handled
            || _container.IsEntityInContainer(uid))
            return;

        _throwing.TryThrow(uid, args.Target, args.JumpSpeed, uid, 10F);
        // todo PlayPVS
        args.Handled = true;
    }

    private void OnHit(EntityUid uid, WerewolfBasicAbilitiesComponent comp, ThrowDoHitEvent args)
    {
        if (args.Handled)
            return;

        _throwingItem.StopThrow(uid, args.Component);

        if (Transform(args.Target).Anchored)
            _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(1));
        else
            _stun.TryKnockdown(args.Target, TimeSpan.FromSeconds(1), true);

        args.Handled = true;
    }
    #endregion

    #region store related shit
    /// <summary>
    /// Deletes and replaces the args.OldActionId with the args.NewActionId, also adding it to the mind
    /// </summary>
    private void OnUpgradeAbility(EntityUid uid, WerewolfBasicAbilitiesComponent comp, EventWerewolfUpgradeAbility args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out _)
            || !TryComp<WerewolfMindComponent>(mindId, out var mindComp))
            return;

        // update the mind to have those new actions
        if (args.OldActionId != null)
            mindComp.UnlockedActions.Remove(args.OldActionId);
        if (!mindComp.UnlockedActions.Contains(args.NewActionId))
            mindComp.UnlockedActions.Add(args.NewActionId);

        SyncActions(uid, comp);

        _popup.PopupEntity(Loc.GetString("werewolf-ability-upgraded"), uid, uid);
        args.Handled = true;
    }

    // used for polymorph ent recieving actions from the mind
    public void SyncActions(EntityUid uid, WerewolfBasicAbilitiesComponent comp) // todo the SERVER gives out an error when you polymorph, tries to remove shit that isnt there, fix before merg Attempted to remove an action Howl (9413/n9413, ActionWerewolfHowl) from an entity that it was never attached to: wolf
    {
        // foreach (var actionEnt in comp.ActionEntities.Values)
        //     if (TryComp<ActionComponent>(actionEnt, out var actComp) && actComp.AttachedEntity == uid) // dont remove stuff from the wolf if it doesnt exist
        //         _actions.RemoveAction(uid, actionEnt);
        foreach (var actionEnt in comp.ActionEntities.Values)
            _actions.RemoveAction(uid, actionEnt);
        comp.ActionEntities.Clear();

        // if the mind has unlocked actions, use those
        if (_mind.TryGetMind(uid, out var mindId, out _) && TryComp<WerewolfMindComponent>(mindId, out var mindComp))
        {
            foreach (var actionId in mindComp.UnlockedActions)
            {
                var actionEnt = _actions.AddAction(uid, actionId);
                if (actionEnt != null)
                    comp.ActionEntities[actionId] = actionEnt.Value;
            }
        }
        else
        {// if not, use starting actions
            foreach (var actionId in comp.WerewolfActions)
            {
                var actionEnt = _actions.AddAction(uid, actionId);
                if (actionEnt != null)
                    comp.ActionEntities[actionId] = actionEnt.Value;
            }
        }
    }
    #endregion
}
