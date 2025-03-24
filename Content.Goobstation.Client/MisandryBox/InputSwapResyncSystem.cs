using Content.Shared.Movement.Systems;
using Robust.Client.GameStates;
using Robust.Client.Timing;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.MisandryBox;

public sealed class InputSwapResyncSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;
    [Dependency] private readonly IClientGameStateManager _stateMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playMan = default!;

    public override void Initialize()
    {
        _stateMan.GameStateApplied += ReapplyModifier;
    }

    public override void Shutdown()
    {
        _stateMan.GameStateApplied -= ReapplyModifier;
    }

    /// This is abysmal dogshit
    /// This exists purely because a component cannot modify another during application state (why the fuck is <see cref="StateApplicationGuard"/> a thing?)
    /// Three approaches - either append comps apply N seconds after round starting
    /// or wait N seconds after ComponentStartup, then reapply
    /// or wait for the GameStateApplied action and dirty the required comps
    private void ReapplyModifier(GameStateAppliedArgs args)
    {
        if (_playMan.LocalSession?.AttachedEntity == null)
            return;

        _move.RefreshMovementSpeedModifiers(_playMan.LocalSession.AttachedEntity.Value);
    }
}
