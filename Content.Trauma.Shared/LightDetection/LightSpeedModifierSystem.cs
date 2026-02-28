using Content.Goobstation.Shared.LightDetection.Systems;
using Content.Shared.Movement.Systems;

namespace Content.Trauma.Shared.LightDetection;

public sealed class LightSpeedModifierSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightSpeedModifierComponent, LightLevelUpdated>(OnLightLevelUpdated);
        SubscribeLocalEvent<LightSpeedModifierComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    private void OnLightLevelUpdated(Entity<LightSpeedModifierComponent> ent, ref LightLevelUpdated args)
    {
        ent.Comp.OnLight = args.NewLightLevel > ent.Comp.RequiredLightLevel;
        Dirty(ent);

        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshMovementSpeed(Entity<LightSpeedModifierComponent> ent,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.OnLight)
        {
            args.ModifySpeed(1f, 1f);
            return;
        }

        args.ModifySpeed(ent.Comp.WalkModifier, ent.Comp.SprintModifier);
    }
}
