using Content.Goobstation.Shared.Wraith.Events;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpikerShuffleSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpikerShuffleComponent, SpikerShuffleEvent>(OnSpikerShuffle);
    }

    private void OnSpikerShuffle(Entity<SpikerShuffleComponent> ent, ref SpikerShuffleEvent args)
    {
        // todo
    }
}
