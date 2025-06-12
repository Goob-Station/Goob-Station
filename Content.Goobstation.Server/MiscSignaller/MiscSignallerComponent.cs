using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.MiscSignaller
{
    [RegisterComponent]
    public sealed partial class MiscSignallerComponent : Component
    {
        [DataField("port", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
        public string Port = "Triggered";
        [DataField]
        public TimeSpan ActivationInterval = TimeSpan.FromSeconds(3);
        public TimeSpan NextActivationWindow;
    }
}