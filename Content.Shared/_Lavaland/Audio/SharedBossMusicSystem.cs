// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Audio;

/// <summary>
/// Plays the boss music on clientside. Use this system on Shared for convenience.
/// </summary>
public abstract class SharedBossMusicSystem : EntitySystem
{
    public virtual void StartBossMusic(Entity<BossMusicComponent?> source)
    {
        if (!Resolve(source.Owner, ref source.Comp, false))
            return;

        StartBossMusic(source.Comp.SoundId);
    }

    public virtual void StartBossMusic(ProtoId<BossMusicPrototype> music) { }

    public virtual void EndAllMusic() { }
}
