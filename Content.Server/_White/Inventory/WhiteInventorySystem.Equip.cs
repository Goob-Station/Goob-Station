using Content.Server._White.Inventory.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Inventory.Events;
using Content.Shared.Whitelist;

namespace Content.Server._White.Inventory;

public sealed partial class WhiteInventorySystem
{
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    private void InitializeEquip()
    {
        SubscribeLocalEvent<EffectsOnEquipComponent, GotEquippedEvent>(OnGotEquipped);
    }

    private void OnGotEquipped(EntityUid uid, EffectsOnEquipComponent component, GotEquippedEvent args)
    {
        if (args.Slot != component.Slot || _entityWhitelist.IsBlacklistPass(component.Blacklist, args.Equipee))
            return;

        var effectsArgs = new EntityEffectBaseArgs(args.Equipee, EntityManager);
        foreach (var effect in component.Effects)
            effect.Effect(effectsArgs);
    }
}
