using System.Numerics;
using System.Threading;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Werewolf.Abilities.Basic;

public sealed class SharedWerewolfBasicAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, HowlEvent>(DoHowl);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, ComponentStartup>(OnStartup);
    }

    public void OnStartup(EntityUid uid, WerewolfBasicAbilitiesComponent comp, ref ComponentStartup args)
    {
        foreach (var actionId in comp.BaseWerewolfActions)
            _actions.AddAction(uid, actionId);

        if (_tag.HasTag(uid, "VulpEmotes"))
        {
            comp.CurrentMutation = "WerewolfTransformWerehuman";
            return;
        }
        comp.CurrentMutation = "WerewolfTransformBasic"; // goida
    }


    # region howl
    private void DoHowl(EntityUid uid, WerewolfBasicAbilitiesComponent comp, ref HowlEvent args) //kill me for copying changeling system please
    {
        if (comp.Transfurmed != true)
        { // cant howl if your not sigma
            _popup.PopupClient(Loc.GetString("werewolf-howl-fail-transfur"), uid);
            return;
        }
        _audio.PlayPredicted(comp.ShriekSound, uid, uid);

        var center = Transform(uid).MapPosition;
        var gamers = Filter.Empty();
        gamers.AddInRange(center, comp.ShriekPower, _player, EntityManager);

        foreach (var gamer in gamers.Recipients)
        {
            if (gamer.AttachedEntity == null)
                continue;

            var pos = Transform(gamer.AttachedEntity!.Value).WorldPosition;
            var delta = center.Position - pos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(.01f, 0);

            _recoil.KickCamera(uid, -delta.Normalized());
            DoFuckingStunIdk((uid, comp));
        }
        // _audio.PlayGlobal(comp.DistantSound, uid); // when you howl, everyone on the station hears a quiet distant howl, which breaks the metashield for the chaplain, "allegedly" todo uncomment when better sound is found
        args.Handled = true;
    }

    private void DoFuckingStunIdk(Entity<WerewolfBasicAbilitiesComponent> ent)
    {
        foreach (var entity in _entityLookup.GetEntitiesInRange(ent.Owner, ent.Comp.ShriekPower))
            _stun.TryParalyze(entity, new TimeSpan(0, 0, 0, ent.Comp.StunDuration), true); //goid
    }
    # endregion
}
