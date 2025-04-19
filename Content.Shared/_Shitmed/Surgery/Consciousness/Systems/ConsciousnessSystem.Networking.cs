using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;

public partial class ConsciousnessSystem
{
    private void InitNet()
    {
        SubscribeLocalEvent<ConsciousnessComponent, ComponentGetState>(OnComponentGet);
        SubscribeLocalEvent<ConsciousnessComponent, ComponentHandleState>(OnComponentHandleState);
    }

    private void OnComponentHandleState(EntityUid uid, ConsciousnessComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ConsciousnessComponentState state)
            return;

        component.Threshold = state.Threshold;
        component.RawConsciousness = state.RawConsciousness;
        component.Multiplier = state.Multiplier;
        component.Cap = state.Cap;
        component.ForceDead = state.ForceDead;
        component.ForceUnconscious = state.ForceUnconscious;
        component.IsConscious = state.IsConscious;
        component.Modifiers.Clear();
        component.Multipliers.Clear();
        component.RequiredConsciousnessParts.Clear();

        foreach (var ((modEntity, modType), modifier) in state.Modifiers)
            component.Modifiers.Add((GetEntity(modEntity), modType), modifier);

        foreach (var ((multiplierEntity, multiplierType), modifier) in state.Multipliers)
            component.Multipliers.Add((GetEntity(multiplierEntity), multiplierType), modifier);

        foreach (var (id, (entity, causesDeath, isLost)) in state.RequiredConsciousnessParts)
            component.RequiredConsciousnessParts.Add(id, (GetEntity(entity), causesDeath, isLost));
    }

    private void OnComponentGet(EntityUid uid, ConsciousnessComponent comp, ref ComponentGetState args)
    {
        var state = new ConsciousnessComponentState();

        state.Threshold = comp.Threshold;
        state.RawConsciousness = comp.RawConsciousness;
        state.Multiplier = comp.Multiplier;
        state.Cap = comp.Cap;
        state.ForceDead = comp.ForceDead;
        state.ForceUnconscious = comp.ForceUnconscious;
        state.IsConscious = comp.IsConscious;

        foreach (var ((modEntity, modType), modifier) in comp.Modifiers)
            if (!TerminatingOrDeleted(modEntity))
                state.Modifiers.Add((GetNetEntity(modEntity), modType), modifier);

        foreach (var ((multiplierEntity, multiplierType), modifier) in comp.Multipliers)
            if (!TerminatingOrDeleted(multiplierEntity))
                state.Multipliers.Add((GetNetEntity(multiplierEntity), multiplierType), modifier);

        foreach (var (id, (entity, causesDeath, isLost)) in comp.RequiredConsciousnessParts)
            if (!TerminatingOrDeleted(entity))
                state.RequiredConsciousnessParts.Add(id, (GetNetEntity(entity), causesDeath, isLost));

        args.State = state;
    }
}
