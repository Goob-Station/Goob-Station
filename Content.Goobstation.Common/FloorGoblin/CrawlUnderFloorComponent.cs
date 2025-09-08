// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.FloorGoblin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CrawlUnderFloorComponent : Component
{
    [DataField]
    public EntityUid? ToggleHideAction;

    [DataField]
    public EntProtoId? ActionProto;

    [DataField]
    public bool Enabled = false;

    [DataField, AutoNetworkedField]
    public List<(string key, int originalMask)> ChangedFixtures = new();

    [DataField, AutoNetworkedField]
    public List<(string key, int originalLayer)> ChangedFixtureLayers = new();

    [DataField]
    public int? OriginalDrawDepth;
}
