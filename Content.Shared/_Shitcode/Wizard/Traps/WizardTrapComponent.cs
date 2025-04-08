// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.Traps;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WizardTrapComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public HashSet<EntityUid> IgnoredMinds = new();

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public bool Triggered;

    [DataField]
    public EntityWhitelist? TargetedEntityWhitelist;

    [DataField]
    public EntityWhitelist IgnoredEntityWhitelist = new();

    [DataField]
    public TimeSpan TimeBetweenTriggers = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public int Charges = 1;

    [DataField]
    public EntProtoId? Effect;

    [DataField]
    public SoundSpecifier? TriggerSound;

    [DataField]
    public bool CanReveal = true;

    [DataField]
    public bool Silent;

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(2);

    [DataField]
    public bool Sparks = true;

    [DataField]
    public float ExamineRange = 1.2f;

    [DataField]
    public int MinSparks = 3;

    [DataField]
    public int MaxSparks = 6;

    [DataField]
    public float MinVelocity = 1f;

    [DataField]
    public float MaxVelocity = 4f;
}

[Serializable, NetSerializable]
public enum TrapVisuals : byte
{
    Alpha,
}