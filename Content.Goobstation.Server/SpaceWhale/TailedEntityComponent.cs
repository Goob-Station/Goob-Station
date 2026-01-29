using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.SpaceWhale
{
    /// <summary>
    /// When given to an entity, creates X tailed entities that try to follow the entity with the component.
    /// </summary>
    [RegisterComponent]
    public sealed partial class TailedEntityComponent : Component
    {
        /// <summary>
        /// amount of entities in between the tail and the head
        /// </summary>
        [DataField] public int Amount = 3;

        /// <summary>
        /// the entity/entities that should be spawned after the head
        /// </summary>
        [DataField(required: true)] public EntProtoId Prototype;

        /// <summary>
        /// How much space between entities
        /// </summary>
        [DataField] public float Spacing = 1f;

        /// <summary>
        /// Speed of tails going their way yk
        /// </summary>
        [DataField] public float Speed = 5f;

        /// <summary>
        /// List of tail segments
        /// </summary>
        [DataField] public List<EntityUid> TailSegments = new();
    }
}
