using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Style
{
    [Serializable, NetSerializable]
    public sealed class StyleHudUpdateEvent : EntityEventArgs
    {
        public StyleRank Rank { get; }
        public float Multiplier { get; }
        public List<string> RecentEvents { get; }

        public StyleHudUpdateEvent(StyleRank rank, float multiplier, List<string> recentEvents)
        {
            Rank = rank;
            Multiplier = multiplier;
            RecentEvents = recentEvents;
        }
    }
    [Serializable, NetSerializable]
    public sealed class StyleRankChangedEvent : EntityEventArgs
    {
        public NetEntity Entity { get; }
        public StyleRank OldRank { get; }
        public StyleRank NewRank { get; }
        public float NewMultiplier { get; }
        public List<string> RecentEvents { get; }

        public StyleRankChangedEvent(NetEntity entity, StyleRank oldRank, StyleRank newRank, float newMultiplier, List<string> recentEvents)
        {
            Entity = entity;
            OldRank = oldRank;
            NewRank = newRank;
            NewMultiplier = newMultiplier;
            RecentEvents = recentEvents;
        }
    }
    [Serializable, NetSerializable]
    public sealed class StyleEventMessage : EntityEventArgs
    {
        public string EventId { get; }

        public StyleEventMessage(string eventId)
        {
            EventId = eventId;
        }
    }

    [Serializable]
    public sealed class UpdateStyleEvent : EntityEventArgs
    {
    }

    public enum StyleRank
    {
        F,
        D,
        C,
        B,
        A,
        S,
        SS,
        SSS,
        R
    }
}
