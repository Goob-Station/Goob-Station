// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Goobstation.Shared.Blob.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent, Access(typeof(SharedBlobTileSystem))]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobTileComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Core;

    [ViewVariables, AutoNetworkedField]
    public ProtoId<BlobTilePrototype> TilePrototype;

    [DataField]
    public bool Refudable = true;

    [DataField]
    public DamageSpecifier HealthOfPulse = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", -4 },
            { "Slash", -4 },
            { "Piercing", -4 },
            { "Heat", -4 },
            { "Cold", -4 },
            { "Shock", -4 },
        }
    };
}
