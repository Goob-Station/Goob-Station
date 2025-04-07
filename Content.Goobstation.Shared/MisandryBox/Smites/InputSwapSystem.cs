using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public sealed class InputSwapSystem : ToggleableSmiteSystem<InputSwapComponent>
{
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;

    public override void Set(EntityUid ent)
    {
        if (!TryComp<MovementSpeedModifierComponent>(ent, out var mod))
            return; // womp

        _move.ChangeBaseSpeed(ent, -mod.BaseWalkSpeed, -mod.BaseSprintSpeed, mod.Acceleration);
        _move.RefreshMovementSpeedModifiers(ent);
    }
}
