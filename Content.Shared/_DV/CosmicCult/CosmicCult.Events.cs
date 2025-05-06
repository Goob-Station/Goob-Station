using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public sealed partial class CosmicSiphonIndicatorEvent(NetEntity target) : EntityEventArgs
{
    public NetEntity Target = target;

    public CosmicSiphonIndicatorEvent() : this(new())
    {
    }
}

public sealed partial class CosmicCultLeadChangedEvent() : EntityEventArgs
{
}

public sealed partial class CosmicCultAddedCultistEvent(): EntityEventArgs
{
}