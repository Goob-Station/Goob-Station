// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Content.Goobstation.Common.Standing;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared._White.Standing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LayingDownComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan StandingUpTime { get; set; } = TimeSpan.FromSeconds(1.5f); // Einstein Engines - Crawling Under Tables

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float LyingSpeedModifier = .3f,
                CrawlingUnderSpeedModifier = 0.5f; // Einstein Engines - Crawling Under Tables

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public bool AutoGetUp = true;

    // Einstein Engines begin
    /// <summary>
    ///     If true, the entity is choosing to crawl under furniture. This is purely visual and has no effect on physics.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public bool IsCrawlingUnder = false;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public int NormalDrawDepth = (int)DrawDepth.DrawDepth.Mobs,
                CrawlingUnderDrawDepth = (int)DrawDepth.DrawDepth.SmallMobs;
}

[Serializable, NetSerializable]
public sealed class ChangeLayingDownEvent : CancellableEntityEventArgs;

public sealed class CheckAutoGetUpEvent : EntityEventArgs;
