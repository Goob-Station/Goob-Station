// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 absurd-shaman <165011607+absurd-shaman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ReverseBearTrap;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReverseBearTrapComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CountdownDuration; //Seconds

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    [DataField, AutoNetworkedField]
    public bool Ticking;

    [DataField, AutoNetworkedField]
    public TimeSpan ActivateTime;

    [DataField, AutoNetworkedField]
    public float CurrentEscapeChance;

    [DataField, AutoNetworkedField]
    public bool Struggling;

    [DataField, AutoNetworkedField]
    public EntityUid? LoopSoundStream { get; set; }

    [DataField("soundPath")]
    public SoundSpecifier LoopSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Machines/clock_tick.ogg");

    [DataField("beepSoundPath")]
    public SoundSpecifier BeepSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Machines/beep.ogg");

    [DataField("snapSoundPath")]
    public SoundSpecifier SnapSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Effects/snap.ogg");

    [DataField]
    public SoundSpecifier StartCuffSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_start.ogg");

    [DataField]
    public List<float>? DelayOptions = null;

    [DataField]
    public float BaseEscapeChance;
}