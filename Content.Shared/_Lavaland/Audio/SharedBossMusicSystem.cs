// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Audio;

/// <summary>
/// Plays the boss music on clientside. Use this system on Shared for convenience.
/// </summary>
public abstract class SharedBossMusicSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public void StartBossMusic(Entity<BossMusicComponent?> source)
    {
        if (!Resolve(source.Owner, ref source.Comp, false))
            return;

        StartBossMusic(source.Comp.SoundId);
    }

    public void StartBossMusic(Entity<BossMusicComponent?> source, EntityUid recipient)
    {
        if (_player.LocalEntity != recipient)
            return;

        StartBossMusic(source);
    }

    public void StartBossMusic(ProtoId<BossMusicPrototype> music, EntityUid recipient)
    {
        if (_player.LocalEntity != recipient)
            return;

        StartBossMusic(music);
    }

    public void EndAllMusic(EntityUid recipient)
    {
        if (_player.LocalEntity != recipient)
            return;

        EndAllMusic();
    }

    public void EndAllMusic(ICommonSession session)
    {
        if (_player.LocalSession != session)
            return;

        EndAllMusic();
    }

    /// <summary>
    /// This method is actually what starts the boss music on client-side.
    /// </summary>
    public virtual void StartBossMusic(ProtoId<BossMusicPrototype> music) { }

    /// <summary>
    /// Ends all boss music for local player if called on client-side.
    /// </summary>
    public virtual void EndAllMusic() { }
}
