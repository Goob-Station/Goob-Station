using Robust.Shared.GameObjects;

namespace Content.Shared.MalfAI.Events
{
    /// <summary>
    /// Raised after an entity is selected as a MalfAI antagonist.
    /// </summary>
    public sealed class AfterMalfAiSelectedEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }

        public AfterMalfAiSelectedEvent(EntityUid entityUid)
        {
            EntityUid = entityUid;
        }
    }
}

