// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Audio;

public sealed class BossMusicSystem : SharedBossMusicSystem
{
    public override void StartBossMusic(ProtoId<BossMusicPrototype> music) { }

    public override void EndAllMusic() { }
}
