// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Shuttles.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedRadarConsoleSystem))]
public sealed partial class RadarConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float RangeVV
    {
        get => MaxRange;
        set => IoCManager
            .Resolve<IEntitySystemManager>()
            .GetEntitySystem<SharedRadarConsoleSystem>()
            .SetRange(Owner, value, this);
    }

    [DataField, AutoNetworkedField]
    public float MaxRange = 256f;

    /// <summary>
    /// If true, the radar will be centered on the entity. If not - on the grid on which it is located.
    /// </summary>
    [DataField]
    public bool FollowEntity = false;
}