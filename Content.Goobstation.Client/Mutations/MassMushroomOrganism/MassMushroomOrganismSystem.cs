using Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Mutations.MassMushroomOrganism;

public sealed class MassMushroomOrganismSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FungalInfectionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<FungalInfectionComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnComponentInit(EntityUid uid, FungalInfectionComponent component, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var rsi = new ResPath("/Textures/_Goobstation/Objects/Specific/Hydroponics/mass_mushroom_organism/mutation_stages.rsi");
        var phases = new (FungalInfectionPhaseVisuals Layer, string State)[]
        {
            (FungalInfectionPhaseVisuals.First,  component.FirstStageVisual),
            (FungalInfectionPhaseVisuals.Second, component.SecondStageVisual),
        };

        foreach (var (layer, state) in phases)
        {
            _spriteSystem.LayerMapReserve((uid, sprite), layer);

            var spec = new SpriteSpecifier.Rsi(rsi, state);

            _spriteSystem.LayerSetSprite((uid, sprite), layer, spec);
            _spriteSystem.LayerSetVisible((uid, sprite), layer, false);
        }
    }

    private void OnAppearanceChange(Entity<FungalInfectionComponent> ent, ref AppearanceChangeEvent args)
    {
        var (uid, _) = ent;

        if (args.Sprite == null)
            return;
        if (!_appearance.TryGetData(uid, FungalInfectionPhaseVisualsStage.InfectionStage, out FungalInfectionPhase stage))
            return;

        var showFirst = stage is FungalInfectionPhase.First or FungalInfectionPhase.Second;
        var showSecond = stage is FungalInfectionPhase.Second;

        _spriteSystem.LayerSetVisible((uid, args.Sprite), FungalInfectionPhaseVisuals.First, showFirst);
        _spriteSystem.LayerSetVisible((uid, args.Sprite), FungalInfectionPhaseVisuals.Second, showSecond);
    }
}
