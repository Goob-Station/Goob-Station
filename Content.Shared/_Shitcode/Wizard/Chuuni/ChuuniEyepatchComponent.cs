// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.Magic.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.Chuuni;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChuuniEyepatchComponent : Component
{
    [DataField]
    public FixedPoint2 HealAmount = 10;

    [DataField]
    public string FlippedPrefix = "flipped";

    [DataField]
    public string MessagePostfix = "-chuuni";

    [DataField, AutoNetworkedField]
    public bool IsFliped;

    [DataField]
    public List<LocId> Backstories = new()
    {
        "chuuni-eyepatch-backstory-1",
        "chuuni-eyepatch-backstory-2",
        "chuuni-eyepatch-backstory-3",
        "chuuni-eyepatch-backstory-4",
    };

    [DataField]
    public Dictionary<MagicSchool, LocId> Invocations = new()
    {
        { MagicSchool.Unset, "chuuni-invocation-unset" },
        { MagicSchool.Holy, "chuuni-invocation-holy" },
        { MagicSchool.Psychic, "chuuni-invocation-psychic" },
        { MagicSchool.Mime, "chuuni-invocation-mime" },
        { MagicSchool.Restoration, "chuuni-invocation-restoration" },
        { MagicSchool.Evocation, "chuuni-invocation-evocation" },
        { MagicSchool.Transmutation, "chuuni-invocation-transmutation" },
        { MagicSchool.Translocation, "chuuni-invocation-translocation" },
        { MagicSchool.Conjuration, "chuuni-invocation-conjuration" },
        { MagicSchool.Necromancy, "chuuni-invocation-necromancy" },
        { MagicSchool.Forbidden, "chuuni-invocation-forbidden" },
        { MagicSchool.Sanguine, "chuuni-invocation-sanguine" },
        { MagicSchool.Chuuni, "chuuni-invocation-chuuni" },
    };

    [DataField, AutoNetworkedField]
    public LocId? SelectedBackstory;

    [DataField]
    public float Delay = 5f;

    [DataField, AutoNetworkedField]
    public float Accumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool CanHeal => Accumulator >= Delay;
}

[Serializable, NetSerializable]
public enum FlippedVisuals : byte
{
    Flipped,
}