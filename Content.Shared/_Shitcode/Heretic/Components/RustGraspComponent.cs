// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using YamlDotNet.Core.Tokens;

namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RustGraspComponent : Component
{
    [DataField]
    public float MinUseDelay = 0.7f;

    [DataField]
    public float MaxUseDelay = 3f;

    [DataField]
    public float CatwalkDelayMultiplier = 0.15f;

    [DataField]
    public string DelayId = "rust";

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";

    [DataField]
    public ProtoId<TagPrototype> CatwalkTag = "Catwalk";
}
