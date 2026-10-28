using Content.Goobstation.Shared.Paint;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.Paint;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class PhotocopierComponent : Component
{
    [DataField]
    public string FirstSlot = "canvas_a";

    [DataField]
    public string SecondSlot = "canvas_b";

    [DataField]
    public TimeSpan ScanTime = TimeSpan.FromSeconds(2.4);

    [DataField]
    public TimeSpan PrintTime = TimeSpan.FromSeconds(2.4);

    [DataField]
    public SoundSpecifier? ScanSound = new SoundPathSpecifier("/Audio/Machines/scanning.ogg");

    [DataField]
    public SoundSpecifier? PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    [ViewVariables]
    public PhotocopierState State = PhotocopierState.Idle;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextStage;

    [ViewVariables]
    public EntityUid? Original;

    [ViewVariables]
    public EntityUid? Target;

    [ViewVariables]
    public EntityUid? User;
}
