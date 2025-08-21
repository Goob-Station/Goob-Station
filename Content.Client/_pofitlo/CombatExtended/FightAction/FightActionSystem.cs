using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.Input;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Robust.Client.Player;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Content.Shared._pofitlo.CombatExtended.FightAction;

namespace Content.Client._pofitlo.CombatExtended.FightAction;
public sealed class FightActionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<FightActionComponent>? FightActionStartup;
    public event Action? FightActionShutdown;
    public event Action<TargetBodyPart>? FightActionChange;
    public event Action<FightActionComponent>? FightActionStatusStartup;
    public event Action<FightActionComponent>? FightActionStatusUpdate;
    public event Action? FightActionStatusShutdown;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FightActionComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<FightActionComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
        SubscribeLocalEvent<FightActionComponent, ComponentStartup>(OnTargetingStartup);
        SubscribeLocalEvent<FightActionComponent, ComponentShutdown>(OnTargetingShutdown);
        SubscribeNetworkEvent<TargetIntegrityChangeEvent>(OnTargetIntegrityChange);
    }

    private void HandlePlayerAttached(EntityUid uid, FightActionComponent component, LocalPlayerAttachedEvent args)
    {
        FightActionStartup?.Invoke(component);
        FightActionStatusStartup?.Invoke(component);
    }

    private void HandlePlayerDetached(EntityUid uid, FightActionComponent component, LocalPlayerDetachedEvent args)
    {
        FightActionShutdown?.Invoke();
        FightActionStatusShutdown?.Invoke();
    }

    private void OnTargetingStartup(EntityUid uid, FightActionComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        FightActionStartup?.Invoke(component);
        FightActionStatusStartup?.Invoke(component);
    }

    private void OnTargetingShutdown(EntityUid uid, FightActionComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        FightActionShutdown?.Invoke();
        FightActionStatusShutdown?.Invoke();
    }

    private void OnTargetIntegrityChange(TargetIntegrityChangeEvent args)
    {
        if (!TryGetEntity(args.Uid, out var uid)
            || !_playerManager.LocalEntity.Equals(uid)
            || !TryComp(uid, out FightActionComponent? component)
            || !args.RefreshUi)
            return;

        FightActionStatusUpdate?.Invoke(component);
    }

    private void HandleTargetChange(ICommonSession? session, TargetBodyPart target)
    {
        if (session == null
            || session.AttachedEntity is not { } uid
            || !TryComp<FightActionComponent>(uid, out var targeting)) // TODO переименовать
            return;

        FightActionChange?.Invoke(target);
    }

}
