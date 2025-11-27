using Content.Shared.Silicons.Laws;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CustomLawboard;

[Serializable]
[NetSerializable]
public enum CustomLawboardUiKey : byte
{
    Key
}


[Serializable]
[NetSerializable]
public sealed class CustomLawboardChangeLawsMessage : BoundUserInterfaceMessage
{
    public List<SiliconLaw> Laws { get; }

    public CustomLawboardChangeLawsMessage(List<SiliconLaw> laws)
    {
        Laws = laws;
    }
}
