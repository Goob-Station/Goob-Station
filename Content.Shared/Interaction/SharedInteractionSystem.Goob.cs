using Content.Goobstation.Common.CCVar;
using Content.Shared._ST.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared.Interaction;

public abstract partial class SharedInteractionSystem
{
    [Dependency] private INetManager _net = default!;

    [Dependency] private IConfigurationManager _cfg = default!;

    private bool _interactionParticlesSuppressed;
    private bool _interactionParticlesEnabled = true;

    private void InitializeInteractionParticles()
    {
        Subs.CVar(_cfg, GoobCVars.InteractionParticlesEnabled, v => _interactionParticlesEnabled = v, true);
    }

    public void SetInteractionParticlesSuppressed(bool suppressed)
    {
        _interactionParticlesSuppressed = suppressed;
    }

    public void DoContactInteraction(EntityUid uidA,
        EntityUid? uidB,
        EntityUid? used,
        bool predicted,
        HandledEntityEventArgs? args = null,
        bool interactionParticles = true,
        StellarInteractionParticleType interactionParticleType = StellarInteractionParticleType.Use)
    {
        DoContactInteraction(uidA, uidB, args);

        if (!interactionParticles || !_interactionParticlesEnabled || _interactionParticlesSuppressed || uidB is not { } target || args?.Handled == false || uidA == target)
            return;

        if (!TryComp(uidA, out MetaDataComponent? metaA) || metaA.EntityPaused
            || !TryComp(target, out MetaDataComponent? metaB) || metaB.EntityPaused
            || HasComp<VirtualItemComponent>(target))
            return;

        if (metaA.CreationTick == _gameTiming.CurTick || metaB.CreationTick == _gameTiming.CurTick)
            return;

        if (_net.IsServer)
        {
            RaiseNetworkEvent(new StellarInteractionParticleEvent(GetNetEntity(uidA), GetNetEntity(used), GetNetEntity(target), false, interactionParticleType), Filter.Pvs(uidA, entityManager: EntityManager));
        }
        else if (predicted && _gameTiming.IsFirstTimePredicted)
        {
            RaiseLocalEvent(new StellarInteractionParticleEvent(GetNetEntity(uidA), GetNetEntity(used), GetNetEntity(target), true, interactionParticleType));
        }
    }
}
