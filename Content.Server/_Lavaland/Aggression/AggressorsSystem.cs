using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Audio;
using Robust.Shared.Player;

namespace Content.Server._Lavaland.Aggression;

public sealed class AggressorsSystem : SharedAggressorsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, AggressorAddedEvent>(OnAgressorAdded);
    }

    private void OnAgressorAdded(Entity<AggressiveComponent> ent, ref AggressorAddedEvent args)
    {

    }
}
