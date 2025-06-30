// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Damage;

[RegisterComponent]
public sealed partial class HierophantClubItemComponent : Component
{
    [DataField]
    public EntProtoId CreateCrossActionId = "ActionHierophantSpawnCross";

    [DataField]
    public EntProtoId PlaceMarkerActionId = "ActionHierophantPlaceMarker";

    [DataField]
    public EntProtoId TeleportToMarkerActionId = "ActionHierophantTeleport";

    [DataField]
    public EntProtoId ToggleTileMovementActionId = "ActionHierophantTileMovement";
    [DataField]
    public EntityUid? CreateCrossActionEntity;

    [DataField]
    public EntityUid? PlaceMarkerActionEntity;

    [DataField]
    public EntityUid? TeleportToMarkerActionEntity;

    [DataField]
    public EntityUid? ToggleTileMovementActionEntity;

    [DataField]
    public EntityUid? TeleportMarker;

    [DataField]
    public EntProtoId TeleportMarkerPrototype = "LavalandHierophantClubTeleportMarker";

    [DataField]
    public float CrossRange = 5f;

    [DataField]
    public SoundSpecifier DamageSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/hiero_blast.ogg");

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");
}