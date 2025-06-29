// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.TapeRecorder;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedTapeRecorderSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class TapeRecorderComponent : Component
{
    /// <summary>
    /// The current tape recorder mode, controls what using the item will do
    /// </summary>
    [DataField, AutoNetworkedField]
    public TapeRecorderMode Mode = TapeRecorderMode.Stopped;

    /// <summary>
    /// Paper that will spawn when printing transcript
    /// </summary>
    [DataField]
    public EntProtoId PaperPrototype = "TapeRecorderTranscript";

    /// <summary>
    /// How fast can this tape recorder rewind
    /// Acts as a multiplier for the frameTime
    /// </summary>
    [DataField]
    public float RewindSpeed = 3f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CooldownEndTime = TimeSpan.Zero;

    /// <summary>
    /// Cooldown of print button
    /// </summary>
    [DataField]
    public TimeSpan PrintCooldown = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Default name as fallback if a message doesn't have one.
    /// </summary>
    [DataField]
    public LocId DefaultName = "tape-recorder-voice-unknown";

    /// <summary>
    /// Sound on print transcript
    /// </summary>
    [DataField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f).WithMaxDistance(3f)
    };

    /// <summary>
    /// What sound is used when play mode is activated
    /// </summary>
    [DataField]
    public SoundSpecifier PlaySound = new SoundPathSpecifier("/Audio/DeltaV/Items/TapeRecorder/play.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f).WithMaxDistance(3f)
    };

    /// <summary>
    /// What sound is used when stop mode is activated
    /// </summary>
    [DataField]
    public SoundSpecifier StopSound = new SoundPathSpecifier("/Audio/DeltaV/Items/TapeRecorder/stop.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f).WithMaxDistance(3f)
    };

    /// <summary>
    /// What sound is used when rewind mode is activated
    /// </summary>
    [DataField]
    public SoundSpecifier RewindSound = new SoundPathSpecifier("/Audio/DeltaV/Items/TapeRecorder/rewind.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f).WithMaxDistance(3f)
    };
}
