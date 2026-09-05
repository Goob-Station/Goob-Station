using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.EntityEffects;

/// <summary>
/// Spawn lava and fire along path they walk.
/// </summary>
public sealed partial class HellstepEntityEffectSystem : EntityEffectSystem<MetaDataComponent, HellstepEffect>
{
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<HellstepEffect> args)
    {
        var hellstep = EnsureComp<HellstepComponent>(entity);
        hellstep.FirePrototype = args.Effect.FirePrototype;
        hellstep.LavaPrototype = args.Effect.LavaPrototype;
    }
}

public sealed partial class HellstepEffect : EntityEffectBase<HellstepEffect>
{
    [DataField]
    public EntProtoId FirePrototype = "HereticFireAA";

    [DataField]
    public EntProtoId LavaPrototype = "FloorLavaEntityTemporary";

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;
}
