using Content.Shared._Lavaland.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Audio;

public sealed class BossMusicSystem : SharedBossMusicSystem
{
    public override void StartBossMusic(ProtoId<BossMusicPrototype> music) { }

    public override void EndAllMusic() { }
}
