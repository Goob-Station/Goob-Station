using Content.Server.Emp;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Content.Shared.Tools.Systems;
using Robust.Shared.Random;

namespace Content.Server.Weapons.Melee.EnergySword;

public sealed class EnergySwordSystem : EntitySystem
{
    [Dependency] private readonly SharedRgbLightControllerSystem _rgbSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnergySwordComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EnergySwordComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<EnergySwordComponent, EmpPulseEvent>(OnEmp); // Goobstation
    }

    private void OnEmp(Entity<EnergySwordComponent> ent, ref EmpPulseEvent args) // Goobstation
    {
        args.Affected = true;
        args.Disabled = true;
        _toggle.TryDeactivate(ent.Owner);
    }

    // Used to pick a random color for the blade on map init.
    private void OnMapInit(EntityUid uid, EnergySwordComponent comp, MapInitEvent args)
    {
        if (comp.ColorOptions.Count != 0)
            comp.ActivatedColor = _random.Pick(comp.ColorOptions);

        if (!TryComp(uid, out AppearanceComponent? appearanceComponent))
            return;
        _appearance.SetData(uid, ToggleableLightVisuals.Color, comp.ActivatedColor, appearanceComponent);
    }

    // Used to make the make the blade multicolored when using a multitool on it.
    private void OnInteractUsing(EntityUid uid, EnergySwordComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_toolSystem.HasQuality(args.Used, SharedToolSystem.PulseQuality))
            return;

        args.Handled = true;
        comp.Hacked = !comp.Hacked;

        if (comp.Hacked)
        {
            var rgb = EnsureComp<RgbLightControllerComponent>(uid);
            _rgbSystem.SetCycleRate(uid, comp.CycleRate, rgb);
        }
        else
            RemComp<RgbLightControllerComponent>(uid);
    }
}
