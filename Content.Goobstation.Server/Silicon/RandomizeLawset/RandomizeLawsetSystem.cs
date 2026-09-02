using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Silicon.RandomizeLawset;

/// <summary>
/// This is giving AI random laws just give the component to an
/// </summary>
public sealed class RandomizeAiLawsetSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    

    public override void Initialize()
    {
        SubscribeLocalEvent<RandomizeLawsetComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RandomizeLawsetComponent, ComponentStartup>(OnComponentStartup);
    }
    
    private void OnMapInit(Entity<RandomizeLawsetComponent> randomAiLawset, ref MapInitEvent args)
    {
        if (string.IsNullOrEmpty(randomAiLawset.Comp.WeightedId.Id)
           || !_proto.TryIndex(randomAiLawset.Comp.WeightedId, out var weightedProto))
            return;
        var randomLawset = weightedProto.Pick(_random);
        EnsureComp<SiliconLawProviderComponent>(randomAiLawset, out var comp);
        comp.Laws = randomLawset;
    }

    private void OnComponentStartup(Entity<RandomizeLawsetComponent> randomAiLawset, ref ComponentStartup args)
    {
        if (string.IsNullOrEmpty(randomAiLawset.Comp.WeightedId.Id)
           || !_proto.TryIndex(randomAiLawset.Comp.WeightedId, out var weightedProto))
            return;
        var randomLawset = weightedProto.Pick(_random);
        if (EnsureComp<SiliconLawProviderComponent>(randomAiLawset, out var comp))
            return;
        comp.Laws = randomLawset;
    }
}