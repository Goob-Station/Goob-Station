// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Mech.Malfunctions;
using Content.Shared.Random;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mech;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechMalfunctionComponent : Component
{
    /// <summary>
    /// How much “health” must the mech have left to try add a malfunction.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float IntegrityPoint = 0.5f;

    /// <summary>
    /// Probability of malfunction to add.
    /// </summary>
    [DataField]
    public float MalfunctionProbability = 0.3f;

    /// <summary>
    /// How much we should increase the number of firestacks for the pilot by for more damage
    /// </summary>
    [DataField]
    public int FirestacksPilotMultiplier = 3;

    /// <summary>
    /// How much firestacks will mech gain after CabinOnFire malfunction.
    /// </summary>
    [DataField]
    public float MechFirestacks = 1f;

    [DataField]
    public ProtoId<WeightedRandomPrototype> MalfunctionWeights = "MechMalfunctionWeights";

    [DataField, ViewVariables, NonSerialized]
    public Dictionary<string, BaseMalfunctionEvent> Malfunctions = new()
    {
        { "ShortCircuit", new ShortCircuitEvent() },
        { "CabinOnFire", new CabinOnFireEvent() },
        { "EngineBroken", new EngineBrokenEvent() },
        { "CabinBreach", new CabinBreachEvent() },
        { "EquipmentLoss", new EquipmentLossEvent() },
    };

}
