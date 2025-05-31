// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Mood.Prototypes;

[Prototype]
[Serializable, NetSerializable]
public sealed partial class MobMoodletPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public bool HideWhenNeutral { get; set; }

    [DataField]
    public float MaxValue { get; set; } = 100f;

    [ViewVariables]
    [DataField]
    public float CurrentValue { get; set; } = 0f;

    [ViewVariables]
    [DataField]
    public float ValueDecayPerSecond { get; set; } = 1f;

    [DataField]
    public MobMoodletModifierEventListenerId ModifierEvent { get; set; }

    [DataField]
    public string NeutralDescriptionLoc { get; set; } = default!;

    [DataField]
    public string[] DescriptionLocs { get; set; } = default!;

    /// <summary>
    /// Exists for optimisation, only check the number when it is requested instead of calculating decay every frame.
    /// </summary>
    [DataField]
    public DateTime? LastChecked { get; set; } = null;

    /// <summary>
    /// If false, mood can increase by events, and will gradually decrease. If true, mood will naturally increase and can be lowered through events.
    /// </summary>
    [DataField]
    public bool InvertedValue { get; set; }
}

[Serializable]
public enum MobMoodletModifierEventListenerId
{
    Damaged,
    Healed,
    EatFood,
    Drink,
}
