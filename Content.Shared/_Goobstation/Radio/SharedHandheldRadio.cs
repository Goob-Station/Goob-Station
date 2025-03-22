using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Radio;

[Serializable, NetSerializable]
public enum HandheldRadioUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class HandheldRadioBoundUIState : BoundUserInterfaceState
{
    public bool MicEnabled;
    public bool SpeakerEnabled;

    public HandheldRadioBoundUIState(bool micEnabled, bool speakerEnabled)
    {
        MicEnabled = micEnabled;
        SpeakerEnabled = speakerEnabled;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleHandheldRadioMicMessage : BoundUserInterfaceMessage
{
    public bool Enabled;

    public ToggleHandheldRadioMicMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleHandheldRadioSpeakerMessage : BoundUserInterfaceMessage
{
    public bool Enabled;

    public ToggleHandheldRadioSpeakerMessage(bool enabled)
    {
        Enabled = enabled;
    }
}
