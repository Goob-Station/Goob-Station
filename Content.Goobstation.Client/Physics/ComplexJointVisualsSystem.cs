// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Physics;

public sealed class ComplexJointVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new ComplexJointVisualsOverlay(EntityManager, _protoMan, _timing));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<ComplexJointVisualsOverlay>();
    }
}
