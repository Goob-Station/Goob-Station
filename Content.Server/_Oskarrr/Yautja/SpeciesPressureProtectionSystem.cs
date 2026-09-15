using Content.Shared._Oskarrr.Yautja.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Humanoid;
using Robust.Shared.Containers;

namespace Content.Server._Oskarrr.Yautja;

/// <summary>
/// Скидає герметичність одягу, якщо раса носія не в whitelist.
/// </summary>
public sealed class SpeciesPressureProtectionSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpeciesPressureProtectionComponent, GetPressureProtectionValuesEvent>(OnGetPressureProtection);
    }

    private void OnGetPressureProtection(
        Entity<SpeciesPressureProtectionComponent> ent,
        ref GetPressureProtectionValuesEvent args)
    {
        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container)
            && TryComp<HumanoidAppearanceComponent>(container.Owner, out var humanoid)
            && ent.Comp.Species.Contains(humanoid.Species))
        {
            return;
        }

        // Не Яутжа (або не екіпіровано) — слот не герметичний.
        args.HighPressureMultiplier = 1f;
        args.HighPressureModifier = 0f;
        args.LowPressureMultiplier = 1f;
        args.LowPressureModifier = 0f;
    }
}
