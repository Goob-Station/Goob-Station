using Content.Shared.Movement.Components;

namespace Content.Shared._Goobstation.MisandryBox.Smites;

public sealed class RunWalkSwapSystem : ToggleableSmiteSystem<RunWalkSwapComponent>
{
    public override void Set(EntityUid owner)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(owner);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseWalkSpeed, movementSpeed.BaseSprintSpeed);
    }
}
