using Content.Shared.Input;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Sprinting;

/// <summary>
/// This handles sprinting code.
/// makle cvar
/// </summary>
public sealed class SharedSprintingSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SprinterComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Sprint, InputCmdHandler.FromDelegate(HandleSprintInput, handle: false,
                outsidePrediction: true))
            .Register<SharedSprintingSystem>();
    }

    private void OnRefreshSpeed(Entity<SprinterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.IsSprinting)
            args.ModifySpeed(1.5f);
    }

    private void HandleSprintInput(ICommonSession? session)
    {
        if (session?.AttachedEntity == null ||
            !TryComp<SprinterComponent>(session.AttachedEntity, out var sprinterComponent))
            return;

        sprinterComponent.IsSprinting = !sprinterComponent.IsSprinting;
        _movementSpeed.RefreshMovementSpeedModifiers(session.AttachedEntity.Value);
    }
}
