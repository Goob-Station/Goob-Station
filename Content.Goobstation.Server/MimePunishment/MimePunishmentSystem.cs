using Content.Goobstation.Common.Speech;
using Content.Goobstation.Server.Mimery;
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
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MimePowersComponent, BreakVowAlertEvent>(OnMimePunish);
    }

    private void OnMimePunish(Entity<MimePowersComponent> ent, ref BreakVowAlertEvent args)
    {
        if (HasComp<MimeryPowersComponent>(ent) || _rand.Prob(args.PunishChance))
            Punish(ent);
    }

    private void Punish(EntityUid ent)
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
