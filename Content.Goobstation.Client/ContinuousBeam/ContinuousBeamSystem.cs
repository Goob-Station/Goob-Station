// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Goobstation.Shared.ContinuousBeam;

namespace Content.Goobstation.Client.ContinuousBeam;

public sealed class ContinuousBeamSystem : SharedContinuousBeamSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new ContinuousBeamOverlay(EntityManager, _prototype, _timing));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<ContinuousBeamOverlay>();
    }
}