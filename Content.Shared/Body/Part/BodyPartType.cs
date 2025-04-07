using Content.Shared.Body.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Body.Part
{
    /// <summary>
    ///     Defines the type of a <see cref="BodyComponent"/>.
    /// </summary>
    [Serializable, NetSerializable]
    public enum BodyPartType: byte
    {
        Other = 0,
        Chest,
        Groin,
        Head,
        Arm,
        Hand,
        Leg,
        Foot,
        Tail
    }
}
