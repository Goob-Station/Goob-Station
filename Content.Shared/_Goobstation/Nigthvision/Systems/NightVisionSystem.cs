using Content.Shared.NightVision.Components;
using Content.Shared.Inventory;
using Content.Shared.Actions;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared.NightVision.Systems;

public sealed class NightVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        if(_net.IsServer)
            SubscribeLocalEvent<NightVisionComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<NightVisionComponent, NVInstantActionEvent>(OnActionToggle);
    }

    [ValidatePrototypeId<EntityPrototype>]
    private const string SwitchNightVisionAction = "SwitchNightVision";

    private void OnComponentStartup(EntityUid uid, NightVisionComponent component, ComponentStartup args)
    {
        if (component.IsToggle)
            _actionsSystem.AddAction(uid, ref component.ActionContainer, SwitchNightVisionAction);
    }

    private void OnActionToggle(EntityUid uid, NightVisionComponent component, NVInstantActionEvent args)
    {
        component.IsNightVision = !component.IsNightVision;
        var changeEv = new NightVisionnessChangedEvent(component.IsNightVision);
        RaiseLocalEvent(uid, ref changeEv);
        Dirty(uid, component);
        _actionsSystem.SetCooldown(component.ActionContainer, TimeSpan.FromSeconds(1));
        if (component.IsNightVision && component.PlaySoundOn)
        {
            if (_net.IsServer)
                _audioSystem.PlayPvs(component.OnOffSound, uid);
        }
    }

    [PublicAPI]
    public void UpdateIsNightVision(EntityUid uid, NightVisionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        var old = component.IsNightVision;


        var ev = new CanVisionAttemptEvent();
        RaiseLocalEvent(uid, ev);
        component.IsNightVision = ev.NightVision;

        if (old == component.IsNightVision)
            return;

        var changeEv = new NightVisionnessChangedEvent(component.IsNightVision);
        RaiseLocalEvent(uid, ref changeEv);
        Dirty(uid, component);
    }
}

[ByRefEvent]
public record struct NightVisionnessChangedEvent(bool NightVision);


public sealed class CanVisionAttemptEvent : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public bool NightVision => Cancelled;
    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.MASK | SlotFlags.HEAD;
}
