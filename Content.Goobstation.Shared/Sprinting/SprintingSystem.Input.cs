using System.Numerics;
using Content.Shared.Input;
using Content.Shared.Movement.Components;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Sprinting;

public sealed partial class SprintingSystem
{
    public override void Initialize()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Sprint, new SprintInputCmdHandler(this))
            .Register<SprintingSystem>();
    }

    private sealed partial class SprintInputCmdHandler(SprintingSystem system) : InputCmdHandler
    {
        public override bool HandleCmdMessage(IEntityManager entManager, ICommonSession? session, IFullInputCmdMessage message)
        {
            if (session?.AttachedEntity == null)
                return false;

            system.HandleSprintInput(session, message);
            return false;
        }
    }

    private void HandleSprintInput(ICommonSession? session, IFullInputCmdMessage message)
    {
        if (session?.AttachedEntity == null
            || !TryComp<SprinterComponent>(session.AttachedEntity, out var sprinterComponent)
            || !TryComp<InputMoverComponent>(session.AttachedEntity, out var inputMoverComponent)
            || !sprinterComponent.IsSprinting
            // We check this instead of physics so that we can gatekeep sprinting to only work when you are moving intentionally, and not walking.
            && _moverController.GetVelocityInput(inputMoverComponent).Sprinting == Vector2.Zero)
            return;

        RaiseLocalEvent(session.AttachedEntity.Value, new SprintToggleEvent(!sprinterComponent.IsSprinting && message.State == BoundKeyState.Down));
    }
}