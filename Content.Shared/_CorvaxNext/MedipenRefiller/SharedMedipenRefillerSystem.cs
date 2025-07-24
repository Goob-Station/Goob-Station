using Content.Shared.Chemistry;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxNext.MedipenRefiller;

[Serializable, NetSerializable]
public sealed class MedipenFillerRefillMedipenMessage : BoundUserInterfaceMessage
{
    public readonly uint Dosage;
    public readonly Color Color;
    public readonly string Label;

    public MedipenFillerRefillMedipenMessage(uint dosage, Color color, string label)
    {
        Dosage = dosage;
        Color = color;
        Label = label;
    }
}

[Serializable, NetSerializable]
public sealed class MedipenRefillerBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly ContainerInfo? InputContainerInfo;
    public readonly ContainerInfo? MedipenContainerInfo;

    public readonly string PreviewPrototype;

    public MedipenRefillerBoundUserInterfaceState(ContainerInfo? inputContainerInfo, ContainerInfo? medipenContainerInfo, string previewPrototype)
    {
        InputContainerInfo = inputContainerInfo;
        MedipenContainerInfo = medipenContainerInfo;
        PreviewPrototype = previewPrototype;
    }
}

[Serializable, NetSerializable]
public sealed class MedipenRefillerApplySettingsMessage : BoundUserInterfaceMessage
{
    public readonly string Label;
    public readonly int Volume;
    public readonly Color Color;

    public MedipenRefillerApplySettingsMessage(string label, Color color, int volume)
    {
        Label = label;
        Color = color;
        Volume = volume;
    }
}

[Serializable, NetSerializable]
public sealed class MedipenRefillerFillMedipenMessage(int volume) : BoundUserInterfaceMessage
{
    public readonly int Volume = volume;

}

[Serializable, NetSerializable]
public enum MedipenVisualState : byte
{
    State,
}

[Serializable, NetSerializable]
public enum MedipenVisualLayer : byte
{
    Fill,
    Empty,
}

[Serializable, NetSerializable]
public enum MedipenColorLayer : byte
{
    Fill,
    Empty,
}

[Serializable, NetSerializable]
public enum MedipenRefillerUiKey : byte
{
    Key
}
