using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Weapons.Melee;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.Whetstone;

public sealed class WhetstoneSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WhetstoneComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<WhetstoneComponent> stone, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } target || stone.Comp.Uses <= 0 ||
            !TryComp(target, out MeleeWeaponComponent? meleeWeapon) ||
            !HasComp<ItemComponent>(target) || // We don't want to sharpen felinids or vulps
            _entityWhitelist.IsValid(stone.Comp.Blacklist, target) ||
            !_entityWhitelist.IsValid(stone.Comp.Whitelist, target))
            return;

        foreach (var (damageTypeId, value) in stone.Comp.DamageIncrease.DamageDict)
        {
            if (!meleeWeapon.Damage.DamageDict.TryGetValue(damageTypeId, out var defaultDamage) ||
                defaultDamage > stone.Comp.MaximumIncrease)
                continue;

            var newDamage = defaultDamage + value;
            if (newDamage > stone.Comp.MaximumIncrease)
                newDamage = stone.Comp.MaximumIncrease;

            meleeWeapon.Damage.DamageDict[damageTypeId] = newDamage;
        }

        _audio.PlayEntity(stone.Comp.SharpenAudio, Filter.Pvs(target), target, true);
        stone.Comp.Uses--;
        if (stone.Comp.Uses <= 0)
            _appearance.SetData(stone, GenericCultVisuals.State, false);
    }
}
