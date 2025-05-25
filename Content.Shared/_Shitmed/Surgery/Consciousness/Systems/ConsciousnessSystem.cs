using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;

[Virtual]
public sealed partial class ConsciousnessSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PainSystem _pain = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitProcess();
        InitNet();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdatePassedOut(frameTime);
    }
}
