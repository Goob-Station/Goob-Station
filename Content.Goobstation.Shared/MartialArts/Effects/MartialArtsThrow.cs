using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared._White.Grab;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Effects;

public sealed partial class MartialArtsThrowEntityEffectSystem : EntityEffectSystem<TransformComponent, MartialArtsThrow>
{
    [Dependency] private GrabThrownSystem _grabThrowing = default!;
    [Dependency] private SharedTransformSystem _xform = default!;

    protected override void Effect(Entity<TransformComponent> target, ref EntityEffectEvent<MartialArtsThrow> args)
    {
        // Hacky to make this work
        if (!TryComp(target, out MartialArtsComboPerformedOnComponent? comp))
            throw new InvalidOperationException("No MartialArtsComboPerformedOnComponent on target!");

        var performer = comp.Performer;

        var mapPos = _xform.GetMapCoordinates(performer).Position;
        var hitPos = _xform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 1f / dir.Length();

        _grabThrowing.Throw(target, performer, dir, args.Effect.ThrownSpeed);
    }
}

[UsedImplicitly]
public sealed partial class MartialArtsThrow : EntityEffectBase<MartialArtsThrow>
{
    /// <summary>
    /// Throwing speed.
    /// </summary>
    [DataField]
    public float ThrownSpeed = 10;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Placeholder";
}
