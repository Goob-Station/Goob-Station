using Robust.Shared.Serialization;

namespace Content.Shared.Nyanotrasen.Kitchen.Components
{
    public abstract partial class SharedDeepFryerComponent : Component { }

    [Serializable, NetSerializable]
    public enum DeepFryerVisuals : byte
    {
        Bubbling,
    }
}
