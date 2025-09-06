// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 portfiend <109661617+portfiend@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.FloorGoblin
{
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
    }
}
