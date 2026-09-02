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
        SubscribeLocalEvent<RandomizeLawsetComponent, ComponentStartup>(OnStartup);
    }
    
    private void OnStartup(EntityUid uid, RandomizeLawsetComponent randomAiLawset, ComponentStartup args)
    {
        if(string.IsNullOrEmpty(randomAiLawset.WeightedId.Id) 
           || !_proto.TryIndex(randomAiLawset.WeightedId, out var weightedProto))
            return;
        var randomLawset = weightedProto.Pick(_random);
        EnsureComp<SiliconLawProviderComponent>(uid, out var comp);
        comp.Laws = randomLawset;
    }
}
