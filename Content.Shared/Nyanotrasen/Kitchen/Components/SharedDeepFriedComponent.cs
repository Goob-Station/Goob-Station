using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Nyanotrasen.Kitchen.Components
{
    [NetworkedComponent]
    public abstract partial class SharedDeepFriedComponent : Component
    {
        /// <summary>
        /// How deep-fried is this item?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("crispiness")]
        public int Crispiness { get; set; }
    }

    [Serializable, NetSerializable]
    public enum DeepFriedVisuals : byte
    {
        Fried,
    }
}
