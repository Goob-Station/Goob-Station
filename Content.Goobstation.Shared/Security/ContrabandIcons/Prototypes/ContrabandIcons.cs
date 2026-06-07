using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._Goobstation.Security.ContrabandIcons.Prototypes;

[Prototype("contrabandIcon")]
public sealed partial class ContrabandIconPrototype : StatusIconPrototype, IInheritingPrototype
{
        /// <inheritdoc />
        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ContrabandIconPrototype>))]
        public string[]? Parents { get; private set; }

        /// <inheritdoc />
        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; private set; }
}
