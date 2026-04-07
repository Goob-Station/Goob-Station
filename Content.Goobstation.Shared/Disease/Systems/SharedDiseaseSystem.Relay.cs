using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Events;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Disease.Systems;

/// <summary>
/// This handles the relay of events from the host to the diseases
/// </summary>
public partial class SharedDiseaseSystem
{

    private void InitializeRelay()
    {
        SubscribeLocalEvent<DiseaseCarrierComponent, StartCollideEvent>(RelayDiseaseEvent);
        SubscribeLocalEvent<DiseaseCarrierComponent, PullStartedMessage>(RelayDiseaseEvent);
        SubscribeLocalEvent<DiseaseCarrierComponent, InteractHandEvent>(RelayDiseaseEvent);
    }

    private void RefRelayDiseaseEvent<T>(EntityUid uid, DiseaseCarrierComponent component, ref T args) where T : notnull
    {
        RelayEvent((uid, component), ref args);
    }

    private void RelayDiseaseEvent<T>(EntityUid uid, DiseaseCarrierComponent component, T args) where T : notnull
    {
        RelayEvent((uid, component), args);
    }

    private void RelayEvent<T>(Entity<DiseaseCarrierComponent> carrier, ref T args) where T : notnull
    {
        var ev = new DiseaseRelayedEvent<T>(args);

        foreach (var disease in carrier.Comp.Diseases.ContainedEntities)
        {
            RaiseLocalEvent(disease, ev); // raise event on disease
            if(!TryComp<DiseaseComponent>(disease, out var component))
                continue;

            foreach(var effect in component.Effects.ContainedEntities)
                RaiseLocalEvent(effect, ev); // raise on all effects on disease
        }

        args = ev.Args;
    }

    private void RelayEvent<T>(Entity<DiseaseCarrierComponent> carrier, T args) where T : notnull
    {
        var ev = new DiseaseRelayedEvent<T>(args);

        foreach (var disease in carrier.Comp.Diseases.ContainedEntities)
        {
            RaiseLocalEvent(disease, ev); // raise event on disease
            if(!TryComp<DiseaseComponent>(disease, out var component))
                continue;

            foreach(var effect in component.Effects.ContainedEntities)
                RaiseLocalEvent(effect, ev); // raise on all effects on disease
        }

    }

}
/// <summary>
///     Event wrapper for relayed events.
/// </summary>
/// <remarks>
///      This avoids nested Disease relays, and makes it easy to have certain events only handled by the initial
///      target entity. E.g. health based movement speed modifiers should not be handled by a hat, even if that hat
///      happens to be a dead mouse.
/// </remarks>
public sealed class DiseaseRelayedEvent<TEvent> : EntityEventArgs
{
    public TEvent Args;

    public DiseaseRelayedEvent(TEvent args)
    {
        Args = args;
    }
}
