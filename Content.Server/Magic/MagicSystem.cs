// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Chat; // Einstein Engines - Languages
using Content.Shared.Magic;
using Content.Shared.Magic.Events;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Magic;

public sealed class MagicSystem : SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
