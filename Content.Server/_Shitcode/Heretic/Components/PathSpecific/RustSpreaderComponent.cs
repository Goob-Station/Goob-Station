// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RustSpreaderComponent : Component
{
    [NonSerialized]
    public Queue<TileRef> TilesToRust = new();

    [NonSerialized]
    public HashSet<TileRef> ProcessedTiles = new();

    [NonSerialized]
    public HashSet<EntityUid> AffectedDocks = new();

    [DataField]
    public int AmountToRust = 1;

    [DataField]
    public bool Paused;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
