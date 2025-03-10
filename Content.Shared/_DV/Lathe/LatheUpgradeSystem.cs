using Content.Shared.Lathe;

namespace Content.Shared._DV.Lathe;

/// <summary>
/// Applies <see cref="LatheUpgradeComponent"/> modifiers when added to a lathe and removes it.
/// </summary>
public sealed class LatheUpgradeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LatheUpgradeComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<LatheUpgradeComponent> ent, ref MapInitEvent args)
    {
        RemCompDeferred<LatheUpgradeComponent>(ent);

        if (!TryComp<LatheComponent>(ent, out var lathe))
            return;

        if (ent.Comp.MaterialUseMultiplier is {} matMul)
            lathe.MaterialUseMultiplier = matMul;
        if (ent.Comp.TimeMultiplier is {} timeMul)
            lathe.TimeMultiplier = timeMul;

        Dirty(ent, lathe);
    }
}
