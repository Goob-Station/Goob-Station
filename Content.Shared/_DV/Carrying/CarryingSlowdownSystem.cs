using Content.Shared.Movement.Systems;

namespace Content.Shared._DV.Carrying;

public sealed class CarryingSlowdownSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CarryingSlowdownComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
    }

    public void SetModifier(Entity<CarryingSlowdownComponent?> ent, float modifier)
    {
        ent.Comp ??= EnsureComp<CarryingSlowdownComponent>(ent);
        ent.Comp.Modifier = modifier;
        Dirty(ent, ent.Comp);

        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMoveSpeed(Entity<CarryingSlowdownComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.Modifier, ent.Comp.Modifier);
    }
}
