// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Heretic.EntitySystems;
using Content.Shared.EntityEffects;
using Robust.Shared.Audio;

namespace Content.Server.Heretic.Components;

[RegisterComponent, Access(typeof(EldritchInfluenceSystem))]
public sealed partial class EldritchInfluenceComponent : Component
{
    [DataField]
    public bool Spent;

    [DataField]
    public SoundSpecifier? ExamineSound = new SoundCollectionSpecifier("bloodCrawl");

    [DataField]
    public LocId ExamineBaseMessage = "influence-base-message";

    [DataField]
    public int FontSize = 22;

    [DataField]
    public List<LocId> HeathenExamineMessages = new()
    {
        "fracture-examine-message-1",
        "fracture-examine-message-2",
        "fracture-examine-message-3",
        "fracture-examine-message-4",
        "fracture-examine-message-5",
        "fracture-examine-message-6",
        "fracture-examine-message-7",
        "fracture-examine-message-7",
        "fracture-examine-message-8",
        "fracture-examine-message-9",
        "fracture-examine-message-10",
        "fracture-examine-message-11",
        "fracture-examine-message-12",
        "fracture-examine-message-13",
        "fracture-examine-message-14",
        "fracture-examine-message-15",
        "fracture-examine-message-16",
    };

    [DataField]
    public List<List<EntityEffect>> PossibleExamineEffects = new();
}
