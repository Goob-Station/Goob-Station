using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.MobPhases;

namespace Content.Server._Lavaland.MobPhases;

/// <summary>
/// Forwards every phase transition to client. Needed because projectile damage is not predicted.
/// </summary>
public sealed class MobPhaseNetworkingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobPhasesComponent, MobPhaseChangedEvent>(OnPhaseChanged);
    }

    private void OnPhaseChanged(Entity<MobPhasesComponent> ent, ref MobPhaseChangedEvent args)
    {
        var netEntity = GetNetEntity(ent.Owner);
        RaiseNetworkEvent(new MobPhaseChangedNetworkEvent(netEntity, args.OldPhase, args.NewPhase));
    }
}
