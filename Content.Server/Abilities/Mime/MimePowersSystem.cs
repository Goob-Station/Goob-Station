using Content.Server.Popups;
using Content.Shared.Abilities.Mime;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Alert;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared.Speech.Muting;
using Robust.Shared.Random; // Goobstation - Mime Enforcement - half of these are probably not needed and just leftovers from testing but im too lazy to fix it.
using Content.Server.Body.Systems; // Goobstation - Mime Enforcement
using Content.Shared.Administration; // Goobstation - Mime Enforcement
using Content.Shared.Administration.Components; // Goobstation - Mime Enforcement
using Content.Server.Administration.Components; // Goobstation - Mime Enforcement
using Content.Server.Administration.Systems; // Goobstation - Mime Enforcement
using Content.Server.Explosion.EntitySystems; // Goobstation - Mime Enforcement
using Content.Server.Polymorph.Systems; // Goobstation - Mime Enforcement
using Content.Shared.Clothing.Components; // Goobstation - Mime Enforcement
using Content.Shared.Inventory; // Goobstation - Mime Enforcement
using Content.Shared.Interaction.Components; // Goobstation - Mime Enforcement
using Content.Server.Speech.Components; // Goobstation - Mime Enforcement
using Content.Shared.Speech.Components; // Goobstation - Mime Enforcement

namespace Content.Server.Abilities.Mime
{
    public sealed class MimePowersSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;
        [Dependency] private readonly TurfSystem _turf = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IRobustRandom _rand = default!; // Goobstation - Mime Enforcement
        [Dependency] private readonly BodySystem _body = default!; // Goobstation - Mime Enforcement
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!; // Goobstation - Mime Enforcement
        [Dependency] private readonly PolymorphSystem _polymorphSystem = default!; // Goobstation - Mime Enforcement
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!; // Goobstation - Mime Enforcement
        [Dependency] private readonly InventorySystem _inventorySystem = default!; // Goobstation - Mime Enforcement

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MimePowersComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<MimePowersComponent, InvisibleWallActionEvent>(OnInvisibleWall);

            SubscribeLocalEvent<MimePowersComponent, BreakVowAlertEvent>(OnBreakVowAlert);
            SubscribeLocalEvent<MimePowersComponent, RetakeVowAlertEvent>(OnRetakeVowAlert);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            // Queue to track whether mimes can retake vows yet

            var query = EntityQueryEnumerator<MimePowersComponent>();
            while (query.MoveNext(out var uid, out var mime))
            {
                if (!mime.VowBroken || mime.ReadyToRepent)
                    continue;

                if (_timing.CurTime < mime.VowRepentTime)
                    continue;

                mime.ReadyToRepent = true;
                _popupSystem.PopupEntity(Loc.GetString("mime-ready-to-repent"), uid, uid);
            }
        }

        private void OnComponentInit(EntityUid uid, MimePowersComponent component, ComponentInit args)
        {
            EnsureComp<MutedComponent>(uid);
            _alertsSystem.ShowAlert(uid, component.VowAlert);
            _actionsSystem.AddAction(uid, ref component.InvisibleWallActionEntity, component.InvisibleWallAction, uid);
        }

        /// <summary>
        /// Creates an invisible wall in a free space after some checks.
        /// </summary>
        private void OnInvisibleWall(EntityUid uid, MimePowersComponent component, InvisibleWallActionEvent args)
        {
            if (!component.Enabled)
                return;

            if (_container.IsEntityOrParentInContainer(uid))
                return;

            var xform = Transform(uid);
            // Get the tile in front of the mime
            var offsetValue = xform.LocalRotation.ToWorldVec();
            var coords = xform.Coordinates.Offset(offsetValue).SnapToGrid(EntityManager, _mapMan);
            var tile = coords.GetTileRef(EntityManager, _mapMan);
            if (tile == null)
                return;

            // Check if the tile is blocked by a wall or mob, and don't create the wall if so
            if (_turf.IsTileBlocked(tile.Value, CollisionGroup.Impassable | CollisionGroup.Opaque))
            {
                _popupSystem.PopupEntity(Loc.GetString("mime-invisible-wall-failed"), uid, uid);
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("mime-invisible-wall-popup", ("mime", uid)), uid);
            // Make sure we set the invisible wall to despawn properly
            Spawn(component.WallPrototype, _turf.GetTileCenter(tile.Value));
            // Handle args so cooldown works
            args.Handled = true;
        }

        private void OnBreakVowAlert(Entity<MimePowersComponent> ent, ref BreakVowAlertEvent args)
        {
            if (args.Handled)
                return;
            BreakVow(ent, ent);
            args.Handled = true;
        }

        private void OnRetakeVowAlert(Entity<MimePowersComponent> ent, ref RetakeVowAlertEvent args)
        {
            if (args.Handled)
                return;
            RetakeVow(ent, ent);
            args.Handled = true;
        }

        /// <summary>
        /// Break this mime's vow to not speak.
        /// </summary>
        public void BreakVow(EntityUid uid, MimePowersComponent? mimePowers = null)
        {
            if (!Resolve(uid, ref mimePowers))
                return;

            if (!mimePowers.CanBreakVow) // Goobstation
                return;

            if (mimePowers.VowBroken)
                return;

            mimePowers.Enabled = false;
            mimePowers.VowBroken = true;
            mimePowers.VowRepentTime = _timing.CurTime + mimePowers.VowCooldown;
            RemComp<MutedComponent>(uid);
            _alertsSystem.ClearAlert(uid, mimePowers.VowAlert);
            _alertsSystem.ShowAlert(uid, mimePowers.VowBrokenAlert);
            _actionsSystem.RemoveAction(uid, mimePowers.InvisibleWallActionEntity);
            if (_rand.Prob(mimePowers.PunishmentChance)) {Punish(uid);} // Goobstation - Mime Enforcement
        }

        private void Punish(EntityUid ent) // Goobstation - Mime Enforcement
        {
            switch(_rand.Next(8))
            {
                case 0:
                    EnsureComp<KillSignComponent>(ent);
                    break;
                case 1:
                    var coords = _transformSystem.GetMapCoordinates(ent);
                    _explosionSystem.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId, 4, 1, 2, ent, maxTileBreak: 0);
                    _body.GibBody(ent);
                    break;
                case 2:
                    _polymorphSystem.PolymorphEntity(ent, "AdminBreadSmite");
                    break;
                case 3:
                    if (TryComp<InventoryComponent>(ent, out var inventory))
                    {
                        var ears = Spawn("ClothingHeadHatCatEars", Transform(ent).Coordinates);
                        EnsureComp<UnremoveableComponent>(ears);
                        _inventorySystem.TryUnequip(ent, "head", true, true, false, inventory);
                        _inventorySystem.TryEquip(ent, ears, "head", true, true, false, inventory);
                    }
                    break;
                case 4:
                    EnsureComp<MaoistAccentComponent>(ent);
                    EnsureComp<OhioAccentComponent>(ent);
                    break;
                case 5:
                    EnsureComp<BackwardsAccentComponent>(ent);
                    EnsureComp<DementiaAccentComponent>(ent);
                    break;
                case 6:
                    EnsureComp<VulgarAccentComponent>(ent);
                    EnsureComp<BoganAccentComponent>(ent);
                    EnsureComp<MaoistAccentComponent>(ent);
                    break;
                case 7:
                    EnsureComp<VulgarAccentComponent>(ent);
                    EnsureComp<RussianAccentComponent>(ent);
                    EnsureComp<BackwardsAccentComponent>(ent);
                    EnsureComp<OhioAccentComponent>(ent);
                    EnsureComp<DementiaAccentComponent>(ent);
                    EnsureComp<MaoistAccentComponent>(ent);
                    break;
            }
        }

        /// <summary>
        /// Retake this mime's vow to not speak.
        /// </summary>
        public void RetakeVow(EntityUid uid, MimePowersComponent? mimePowers = null)
        {
            if (!Resolve(uid, ref mimePowers))
                return;

            if (!mimePowers.ReadyToRepent)
            {
                _popupSystem.PopupEntity(Loc.GetString("mime-not-ready-repent"), uid, uid);
                return;
            }

            mimePowers.Enabled = true;
            mimePowers.ReadyToRepent = false;
            mimePowers.VowBroken = false;
            AddComp<MutedComponent>(uid);
            _alertsSystem.ClearAlert(uid, mimePowers.VowBrokenAlert);
            _alertsSystem.ShowAlert(uid, mimePowers.VowAlert);
            _actionsSystem.AddAction(uid, ref mimePowers.InvisibleWallActionEntity, mimePowers.InvisibleWallAction, uid);
        }
    }
}
