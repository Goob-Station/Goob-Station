// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization; // Gaby
using Content.Shared.DoAfter; // Gaby

namespace Content.Shared.Stains;

[RegisterComponent, NetworkedComponent]
public sealed partial class StainableComponent : Component
{
    [DataField]
    public string SolutionId = "stain";

    [DataField]
    public FixedPoint2 MaxVolume = 10f;

    [DataField]
    public FixedPoint2 StainVolume = 0.5f;

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> ItemVisuals = new();

    [DataField]
    public List<PrototypeLayerData> IconVisuals = new();

    [ViewVariables]
    public HashSet<int> RevealedIconVisuals = new();

    [DataField]
    public float CleanseDelay = 15f;
}

[Serializable, NetSerializable]
public sealed partial class WringStainDoAfterEvent : SimpleDoAfterEvent
{
}
