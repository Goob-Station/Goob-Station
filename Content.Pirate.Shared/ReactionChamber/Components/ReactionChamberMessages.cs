using System;
using System.Collections.Generic;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.ReactionChamber.Components;

[Serializable]
[NetSerializable]
public sealed class ReactionChamberTempChangeMessage : BoundUserInterfaceMessage
{
    public float Temp;
    public ReactionChamberTempChangeMessage(float temp)
    {
        Temp = temp;
    }
}
[Serializable]
[NetSerializable]
public sealed class ReactionChamberActiveChangeMessage : BoundUserInterfaceMessage
{
    public bool Active;
    public ReactionChamberActiveChangeMessage(bool active)
    {
        Active = active;
    }
}
[Serializable]
[NetSerializable]
public sealed class BeakerInfo
{
    public string? Name;
    public FixedPoint2? Volume;
    public FixedPoint2? MaxVolume;
    public List<ReagentQuantity>? Reagents;
    public FixedPoint2? Temp;
    public FixedPoint2? SpinBoxTemp;
    public BeakerInfo(string? name = null, FixedPoint2? volume = null, FixedPoint2? maxVolume = null, List<ReagentQuantity>? reagents = null, FixedPoint2? temp = null, FixedPoint2? spinBoxTemp = null)
    {
        Name = name;
        MaxVolume = maxVolume;
        Volume = volume;
        Reagents = reagents;
        Temp = temp;
        SpinBoxTemp = spinBoxTemp;
    }
}
[Serializable]
[NetSerializable]
public sealed class ReactionChamberBoundUIState : BoundUserInterfaceState
{
    public readonly NetEntity? Beaker;
    public readonly BeakerInfo? BeakerInfo;

    public ReactionChamberBoundUIState(NetEntity? beaker = null, BeakerInfo? beakerInfo = null)
    {
        Beaker = beaker;
        BeakerInfo = beakerInfo;
    }
}
