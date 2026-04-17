using Content.Goobstation.Common.Mimery;
using Content.Shared.Abilities.Mime;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Magic;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Mimery;

public abstract class SharedAdvancedMimerySystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;

    [Dependency] private readonly SharedHandsSystem _hands = default!;
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

        EnsureComp<MimePowersComponent>(user.Value);
    }

    private void OnFingerGuns(Entity<MimePowersComponent> ent, ref FingerGunsActionEvent args)
    {
        ShootFingerGuns(ent, ref args);
    }

    protected virtual bool ShootFingerGuns(Entity<MimePowersComponent> ent, ref FingerGunsActionEvent args)
    {
        if (args.Handled || !ent.Comp.Enabled)
            return false;

        if (!_hands.TryGetEmptyHand(ent.Owner, out _))
        {
            _popupSystem.PopupClient(Loc.GetString("finger-guns-event-need-hand"), ent, ent);
            return false;
        }

        _magic.OnProjectileSpell(args);

        return args.Handled;
    }

    private void OnInvisibleBlockade(Entity<MimePowersComponent> ent, ref InvisibleBlockadeActionEvent args)
    {
        if (args.Handled || !ent.Comp.Enabled)
            return;

        var transform = Transform(ent);
        foreach (var position in _magic.GetInstantSpawnPositions(transform, new TargetInFront()))
        {
            args.Handled = true;
            PredictedSpawnAttachedTo(ent.Comp.WallPrototype, position.SnapToGrid(EntityManager, _mapMan));
        }

        if (!args.Handled)
            return;

        var messageSelf = Loc.GetString("mime-invisible-wall-popup-self",
            ("mime", Identity.Entity(ent.Owner, EntityManager)));
        var messageOthers = Loc.GetString("mime-invisible-wall-popup-others",
            ("mime", Identity.Entity(ent.Owner, EntityManager)));
        _popupSystem.PopupPredicted(messageSelf, messageOthers, ent, ent);
    }
}
