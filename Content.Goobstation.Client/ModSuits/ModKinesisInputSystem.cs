using System.Numerics;
using Content.Client.Construction;
using Content.Client.Graphics;
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
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModKinesisInputSystem : EntitySystem
{
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    private const float CursorTolerance = 0.1f;

    private const string OutlineShaderId = "mod-kinesis-outline";
    private static readonly ProtoId<ShaderPrototype> OutlineShader = "SelectionOutlineInrange";
    private static readonly Color OutlineColor = Color.FromHex("#9ad8ffcc");

    private bool _holding;
    private Vector2? _lastCursor;
    private readonly HashSet<EntityUid> _outlined = new();
    private readonly List<EntityUid> _stale = new();
    private ShaderInstance? _outlineShader;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(new CommandBind(EngineKeyFunctions.Use,
                new PointerInputCmdHandler(OnUse, ignoreUp: false, outsidePrediction: true),
                before: new[] { typeof(DragDropSystem), typeof(SharedInteractionSystem), typeof(ConstructionSystem) },
                after: new[] { typeof(ActionUIController) }))
            .Register<ModKinesisInputSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<ModKinesisInputSystem>();
    }

    private bool OnUse(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (_player.LocalEntity is not { } player
            || !HasComp<ModKinesisUserComponent>(player)
            || _ui.GetUIController<ActionUIController>().SelectingTargetFor != null)
            return false;

        if (args.State == BoundKeyState.Down)
        {
            if (args.EntityUid.IsValid() && args.EntityUid != player)
            {
                _holding = true;
                _lastCursor = null;
                RaiseNetworkEvent(new ModKinesisGrabEvent
                {
                    Target = GetNetEntity(args.EntityUid),
                    Cursor = GetNetCoordinates(args.Coordinates),
                });
            }

            return true;
        }

        if (_holding)
        {
            _holding = false;
            RaiseNetworkEvent(new ModKinesisReleaseEvent());
        }

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted || !_holding)
            return;

        if (_player.LocalEntity is not { } player || !HasComp<ModKinesisUserComponent>(player))
        {
            _holding = false;
            return;
        }

        if (!_input.MouseScreenPosition.IsValid)
            return;

        var mouse = _eye.PixelToMap(_input.MouseScreenPosition);
        if (mouse.MapId == MapId.Nullspace)
            return;

        if (_lastCursor is { } last && (last - mouse.Position).Length() < CursorTolerance)
            return;

        _lastCursor = mouse.Position;
        RaiseNetworkEvent(new ModKinesisCursorEvent
        {
            Cursor = GetNetCoordinates(_transform.ToCoordinates(mouse)),
        });
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<ModKinesisHeldComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out _, out var sprite))
        {
            if (_outlined.Contains(uid))
                continue;

            _outlined.Add(uid);
            _outlineShader ??= CreateOutlineShader();
            _sprite.SetPostShader((uid, sprite), new SpriteComponent.PostShaderArgs(OutlineShaderId, _outlineShader)
            {
                After = ContentPostShaderIds.AfterBaseEffects,
            });
        }

        _stale.Clear();
        foreach (var uid in _outlined)
        {
            if (!HasComp<ModKinesisHeldComponent>(uid))
                _stale.Add(uid);
        }

        foreach (var uid in _stale)
        {
            _outlined.Remove(uid);
            if (_spriteQuery.TryComp(uid, out var sprite))
                _sprite.RemovePostShader((uid, sprite), OutlineShaderId);
        }
    }

    private ShaderInstance CreateOutlineShader()
    {
        var shader = _prototype.Index(OutlineShader).InstanceUnique();
        shader.SetParameter("outline_color", OutlineColor);
        return shader;
    }
}
