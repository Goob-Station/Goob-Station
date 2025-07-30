using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MindSwappedComponent : Component
    {
        [ViewVariables]
        public EntityUid OriginalEntity = default!;
        [DataField("mindSwapReturnActionId",
        customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? MindSwapReturnActionId = "ActionMindSwapReturn";

        [DataField("mindSwapReturnActionEntity")]
        public EntityUid? MindSwapReturnActionEntity;
    }
}
