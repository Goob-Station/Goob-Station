using Content.Shared.Damage;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Damage;

public sealed partial class SpawnSolutionOnDamageSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = null!;
     public override void Initialize()
    {
        SubscribeLocalEvent<SpawnSolutionOnDamageComponent, BeforeDamageChangedEvent>(OnTakeDamage);
    }
    private void OnTakeDamage(Entity<SpawnSolutionOnDamageComponent> ent, ref BeforeDamageChangedEvent args)
    {

        if (!args.Damage.AnyPositive())
            return;

        if (args.Damage.GetTotal() <= ent.Comp.Threshold)
            return; //dont trigger on low damage

        var probability = Math.Clamp(ent.Comp.Probability, 0f, 1f);
        if(_random.Prob(probability))
            return;

        if (ent.Comp.Solution == "unknown")
            return;

        Spawn(ent.Comp.Solution, Transform(ent).Coordinates);
    }
}
