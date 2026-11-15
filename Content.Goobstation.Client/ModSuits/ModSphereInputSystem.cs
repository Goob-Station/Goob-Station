using System.Numerics;
using Content.Client.Construction;
using Content.Client.Interaction;
using Content.Client.UserInterface.Systems.Actions;
using Content.Goobstation.Shared.ModSuits.Modules;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModSphereInputSystem : EntitySystem
{
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private InputSystem _inputSystem = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private const float AngleTolerance = 0.07f;
    private const float DistanceTolerance = 0.15f;

    private EntityUid? _lastEntity;
    private Vector2 _lastOffset;
    private bool _lastThrusting;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(new CommandBind(EngineKeyFunctions.Use,
                new PointerInputCmdHandler(OnUse, ignoreUp: false, outsidePrediction: true),
                before: new[] { typeof(DragDropSystem), typeof(SharedInteractionSystem), typeof(ConstructionSystem) },
                after: new[] { typeof(ActionUIController) }))
            .Register<ModSphereInputSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<ModSphereInputSystem>();
    }

    private bool OnUse(ICommonSession? session, EntityCoordinates coords, EntityUid uid)
    {
        return _player.LocalEntity is { } player && HasComp<ModSphereFormComponent>(player);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } player || !HasComp<ModSphereFormComponent>(player))
        {
            _lastEntity = null;
            _lastThrusting = false;
            return;
        }

        var thrusting = TryGetCursorOffset(player, out var offset);
        if (_lastEntity == player && thrusting == _lastThrusting && (!thrusting || !Changed(offset)))
            return;

        _lastEntity = player;
        _lastThrusting = thrusting;
        _lastOffset = offset;
        RaisePredictiveEvent(new ModSphereSteerEvent
        {
            Offset = offset,
            Thrusting = thrusting,
        });
    }

    private bool TryGetCursorOffset(EntityUid player, out Vector2 offset)
    {
        offset = Vector2.Zero;
        if (_inputSystem.CmdStates.GetState(EngineKeyFunctions.Use) != BoundKeyState.Down
            || !_input.MouseScreenPosition.IsValid
            || _ui.GetUIController<ActionUIController>().SelectingTargetFor != null)
            return false;

        var mouse = _eye.PixelToMap(_input.MouseScreenPosition);
        var position = _transform.GetMapCoordinates(player);
        if (mouse.MapId == MapId.Nullspace || mouse.MapId != position.MapId)
            return false;

        offset = mouse.Position - position.Position;
        return true;
    }

    private bool Changed(Vector2 offset)
    {
        if (MathF.Abs(offset.Length() - _lastOffset.Length()) > DistanceTolerance)
            return true;

        if (offset.LengthSquared() < 0.0001f || _lastOffset.LengthSquared() < 0.0001f)
            return false;

        var angle = Angle.ShortestDistance(new Angle(offset), new Angle(_lastOffset));
        return Math.Abs(angle.Theta) > AngleTolerance;
    }
}
