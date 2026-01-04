using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Devil;
using Content.Server.Humanoid;
using Content.Shared.Examine;
using Content.Shared.Humanoid;

namespace Content.Goobstation.Server.Cult.Systems;
public sealed partial class BloodCultVisualEyesSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _human = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultVisualEyesComponent, ComponentStartup>(OnCultEyesAdded);
        SubscribeLocalEvent<BloodCultVisualEyesComponent, ComponentShutdown>(OnCultEyesRemoved);
        SubscribeLocalEvent<BloodCultVisualEyesComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<BloodCultVisualEyesComponent> ent, ref ExaminedEvent args)
    {
        var ev = new IsEyesCoveredCheckEvent();
        RaiseLocalEvent(args.Examined, ev);
        if (ev.IsEyesProtected)
            return;

        args.PushMarkup(Loc.GetString("cult-tier-eyes-examine"));
    }

    private void OnCultEyesAdded(Entity<BloodCultVisualEyesComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var huac))
            return;

        ent.Comp.LastEyeColor = huac.EyeColor;
        _human.SetBaseLayerColor(ent.Owner, HumanoidVisualLayers.Eyes, ent.Comp.EyeColor);
    }

    private void OnCultEyesRemoved(Entity<BloodCultVisualEyesComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var huac) || ent.Comp.LastEyeColor == null)
            return;

        _human.SetBaseLayerColor(ent.Owner, HumanoidVisualLayers.Eyes, ent.Comp.LastEyeColor);
    }
}
