// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Cargo;
using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Pirates.GameTicking.Rules;

[RegisterComponent]
public sealed partial class PendingPirateRuleComponent : Component
{
    [DataField] public float PirateSpawnTime = 300f; // 5 minutes
    public float PirateSpawnTimer = 0f;

    [DataField(required: true)] public EntProtoId RansomPrototype;

    // we need this for random announcements otherwise it'd be bland
    [DataField] public string LocAnnouncer = "irs";

    [DataField] public ProtoId<DatasetPrototype>? LocAnnouncers = null;

    [DataField] public float Ransom = 25000f;

    public CargoOrderData? Order;
}