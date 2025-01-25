using Content.Shared.Input;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Robust.Client.Player;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Client._Shitmed.Targeting;
public sealed class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<TargetingComponent>? TargetingStartup;
    public event Action? TargetingShutdown;
    public event Action<TargetBodyPart>? TargetChange;
    public event Action<TargetingComponent>? PartStatusStartup;
    public event Action<TargetingComponent>? PartStatusUpdate;
    public event Action? PartStatusShutdown;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetingComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<TargetingComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
        SubscribeLocalEvent<TargetingComponent, ComponentStartup>(OnTargetingStartup);
        SubscribeLocalEvent<TargetingComponent, ComponentShutdown>(OnTargetingShutdown);
        SubscribeNetworkEvent<TargetIntegrityChangeEvent>(OnTargetIntegrityChange);

        CommandBinds.Builder
        .Bind(ContentKeyFunctions.TargetHead,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.Head)))
        .Bind(ContentKeyFunctions.TargetTorso,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.Torso)))
        .Bind(ContentKeyFunctions.TargetLeftArm,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.LeftArm)))
        .Bind(ContentKeyFunctions.TargetLeftHand,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.LeftHand)))
        .Bind(ContentKeyFunctions.TargetRightArm,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.RightArm)))
        .Bind(ContentKeyFunctions.TargetRightHand,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.RightHand)))
        .Bind(ContentKeyFunctions.TargetLeftLeg,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.LeftLeg)))
        .Bind(ContentKeyFunctions.TargetLeftFoot,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.LeftFoot)))
        .Bind(ContentKeyFunctions.TargetRightLeg,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.RightLeg)))
        .Bind(ContentKeyFunctions.TargetRightFoot,
            InputCmdHandler.FromDelegate((session) => HandleTargetChange(session, TargetBodyPart.RightFoot)))
        .Register<SharedTargetingSystem>();
    }

    private void HandlePlayerAttached(EntityUid uid, TargetingComponent component, LocalPlayerAttachedEvent args)
    {
        TargetingStartup?.Invoke(component);
        PartStatusStartup?.Invoke(component);
    }

    private void HandlePlayerDetached(EntityUid uid, TargetingComponent component, LocalPlayerDetachedEvent args)
    {
        TargetingShutdown?.Invoke();
        PartStatusShutdown?.Invoke();
    }

    private void OnTargetingStartup(EntityUid uid, TargetingComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        TargetingStartup?.Invoke(component);
        PartStatusStartup?.Invoke(component);
    }

    private void OnTargetingShutdown(EntityUid uid, TargetingComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        TargetingShutdown?.Invoke();
        PartStatusShutdown?.Invoke();
    }

    private void OnTargetIntegrityChange(TargetIntegrityChangeEvent args)
    {
        if (!TryGetEntity(args.Uid, out var uid)
            || !_playerManager.LocalEntity.Equals(uid)
            || !TryComp(uid, out TargetingComponent? component)
            || !args.RefreshUi)
            return;

        PartStatusUpdate?.Invoke(component);
    }

    private void HandleTargetChange(ICommonSession? session, TargetBodyPart target)
    {
        if (session == null
            || session.AttachedEntity is not { } uid
            || !TryComp<TargetingComponent>(uid, out var targeting))
            return;

        TargetChange?.Invoke(target);
    }
}
