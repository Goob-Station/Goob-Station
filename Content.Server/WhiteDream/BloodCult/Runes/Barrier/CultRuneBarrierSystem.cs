namespace Content.Server.WhiteDream.BloodCult.Runes.Barrier;

public sealed class CultRuneBarrierSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CultRuneBarrierComponent, TryInvokeCultRuneEvent>(OnBarrierRuneInvoked);
    }

    private void OnBarrierRuneInvoked(Entity<CultRuneBarrierComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        Spawn(ent.Comp.SpawnPrototype, Transform(ent).Coordinates);
        Del(ent);
    }
}
