using Content.Goobstation.Common.Speech;
using Content.Goobstation.Server.Mimery;
using Content.Goobstation.Shared.MimePunishment;
using Content.Server.Abilities.Mime;
using Content.Server.Administration.Components;
using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.Speech.Components;
using Content.Shared.Abilities.Mime;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Speech.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.MimePunishment;

public sealed class MimePunishmentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _rand = default!; // Goobstation - Mime Enforcement
    [Dependency] private readonly BodySystem _body = default!; // Goobstation - Mime Enforcement
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!; // Goobstation - Mime Enforcement
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!; // Goobstation - Mime Enforcement
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!; // Goobstation - Mime Enforcement
    [Dependency] private readonly InventorySystem _inventorySystem = default!; // Goobstation - Mime Enforcement
    public override void Initialize()
    {
        SubscribeLocalEvent<MimePunishmentComponent, MimePunishEvent>(OnMimePunish);
        SubscribeLocalEvent<MimePowersComponent, MapInitEvent>(OnMimePowersInitialized);
        SubscribeLocalEvent<MimeryPowersComponent, MapInitEvent>(OnMimeryPowersInitialized);
    }

    private void OnMimePunish(Entity<MimePunishmentComponent> ent, ref MimePunishEvent args)
    {
        if (_rand.Prob(args.Chance)) {Punish(ent);} // Goobstation - Mime Enforcement
    }

    private void OnMimePowersInitialized(Entity<MimePowersComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<MimePunishmentComponent>(ent);
    }

    private void OnMimeryPowersInitialized(Entity<MimeryPowersComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<MimePunishmentComponent>(ent);
    }

    private void Punish(EntityUid ent) // Goobstation - Mime Enforcement
        {
            if (TryComp<MimeryPowersComponent>(ent, out var mimeryPowersComponent))
                RemComp<MimeryPowersComponent>(ent);

            if (TryComp<FingerGunComponent>(ent, out var fingerGunComponent))
                RemComp<FingerGunComponent>(ent);


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
}
