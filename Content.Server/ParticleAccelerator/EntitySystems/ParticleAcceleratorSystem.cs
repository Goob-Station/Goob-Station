// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Projectiles;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Server.ParticleAccelerator.EntitySystems;

public sealed partial class ParticleAcceleratorSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly ProjectileSystem _projectileSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeControlBoxSystem();
        InitializePartSystem();
        InitializePowerBoxSystem();
    }
}