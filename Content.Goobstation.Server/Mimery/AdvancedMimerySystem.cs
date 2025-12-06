using Content.Goobstation.Common.Mimery;
using Content.Goobstation.Shared.Mimery;
using Content.Server.Abilities.Mime;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Actions;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Magic;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Mimery;

public sealed class AdvancedMimerySystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;

    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MimePowersComponent, InvisibleBlockadeActionEvent>(OnInvisibleBlockade);
        SubscribeLocalEvent<MimePowersComponent, FingerGunsActionEvent>(OnFingerGuns);

        SubscribeLocalEvent<AdvancedMimeryActionComponent, ActionGotAddedEvent>(OnAdd);
    }

    private void OnAdd(Entity<AdvancedMimeryActionComponent> ent, ref ActionGotAddedEvent args)
    {
        EntityUid? user = args.Container;
        if (TryComp(user.Value, out MindComponent? mind))
            user = mind.OwnedEntity;

        if (!HasComp<MobStateComponent>(user))
            return;

        EnsureComp<MimePowersComponent>(user.Value).PunishmentChance = 0f;
    }

    private void OnFingerGuns(Entity<MimePowersComponent> ent, ref FingerGunsActionEvent args)
    {
        if (args.Handled || !ent.Comp.Enabled)
            return;

        if (!TryComp(args.Action.Owner, out FingerGunsActionComponent? actionComp))
            return;

        if (!_hands.TryGetEmptyHand(ent.Owner, out _))
        {
            _popupSystem.PopupEntity(Loc.GetString("finger-guns-event-need-hand"), ent, ent);
            return;
        }

        _magic.OnProjectileSpell(args);

        if (!args.Handled)
            return;

        actionComp.UsesLeft--;
        if (actionComp.UsesLeft > 0)
            _actions.SetUseDelay(args.Action.Owner, actionComp.FireDelay);
        else
        {
            _actions.SetUseDelay(args.Action.Owner, actionComp.UseDelay);
            actionComp.UsesLeft = actionComp.CastAmount;
            RaiseNetworkEvent(new StopTargetingEvent(), ent);
        }
    }

    private void OnInvisibleBlockade(Entity<MimePowersComponent> ent, ref InvisibleBlockadeActionEvent args)
    {
        if (args.Handled || !ent.Comp.Enabled)
            return;

        args.Handled = true;

        var transform = Transform(ent);

        _popupSystem.PopupEntity(Loc.GetString("mime-invisible-wall-popup", ("mime", ent)), ent);
        foreach (var position in _magic.GetInstantSpawnPositions(transform, new TargetInFront()))
        {
            Spawn(ent.Comp.WallPrototype, position.SnapToGrid(EntityManager, _mapMan));
        }
    }
}
