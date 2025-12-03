using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;

[Prototype("contrabandIcon")]
public sealed partial class ContrabandIconPrototype : StatusIconPrototype, IInheritingPrototype
{
        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ContrabandIconPrototype>))]
        public string[]? Parents { get; private set; }

        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; private set; }
}
