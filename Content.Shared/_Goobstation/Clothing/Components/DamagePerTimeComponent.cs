using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Content.Shared.Damage;

namespace Content.Shared._Goobstation.Clothing.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class DamagePerTimeComponent : Component
    {
        [DataField("damage", required: true)]
        public DamageSpecifier Damage { get; set; } = new();

        [DataField("interval", customTypeSerializer: typeof(TimespanSerializer))]
        public TimeSpan Interval = TimeSpan.FromSeconds(1);

        [DataField]
        public TimeSpan NextTickTime = TimeSpan.Zero;
    }
}
