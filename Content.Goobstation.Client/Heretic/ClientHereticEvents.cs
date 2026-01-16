using Content.Shared.Heretic.Prototypes;

namespace Content.Goobstation.Client.Heretic;

public sealed class IconSmoothCornersInitializedEvent : EntityEventArgs;

#region Ritual events

// these do nothing and are there just for yaml limter to shut the fuck up.
// make sure they stay up in sync with the server counterpart.
// regards.
// - john

public sealed partial class RitualAshAscendBehavior : RitualSacrificeBehavior { }
public sealed partial class RitualBladeAscendBehavior : RitualSacrificeBehavior { }
public sealed partial class RitualMuteGhoulifyBehavior : RitualSacrificeBehavior { }

public sealed partial class RitualRustAscendBehavior : RitualSacrificeBehavior { }

public sealed partial class RitualCosmosAscendBehavior : RitualSacrificeBehavior { }

[Virtual]
public partial class RitualSacrificeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualPathBasedSpawnBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualTemperatureBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualReagentPuddleBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualKnowledgeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualCreateBladeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualRecallBladeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualFeastOfOwlsBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}

public sealed partial class RitualCanAscend : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}


#endregion
