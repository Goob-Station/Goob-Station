using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._Goobstation.MisandryBox.Smites;

public sealed class InputSwapSystem : ToggleableSmiteSystem<InputSwapComponent>
{
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;

    public override void Set(EntityUid ent)
    {
        EnsureComp<MovementSpeedModifierComponent>(ent, out var mod);

        _move.ChangeBaseSpeed(ent, -mod.BaseWalkSpeed, -mod.BaseSprintSpeed, mod.Acceleration, mod);

        Dirty(ent, mod);
    }
}
