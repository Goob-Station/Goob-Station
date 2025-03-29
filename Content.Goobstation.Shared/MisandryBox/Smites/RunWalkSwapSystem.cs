using Content.Shared.Movement.Components;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public sealed class RunWalkSwapSystem : ToggleableSmiteSystem<RunWalkSwapComponent>
{
    public override void Set(EntityUid owner)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(owner);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseWalkSpeed, movementSpeed.BaseSprintSpeed);
    }
}
