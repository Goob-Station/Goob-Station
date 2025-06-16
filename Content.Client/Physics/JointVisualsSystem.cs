// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Shared.Prototypes; // Goob edit
using Robust.Shared.Timing; // Goob edit

namespace Content.Client.Physics;

public sealed class JointVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!; // Goob edit
    [Dependency] private readonly IGameTiming _timing = default!; // Goob edit

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new JointVisualsOverlay(EntityManager, _protoMan, _timing)); // Goob edit
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<JointVisualsOverlay>();
    }
}
