using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.WallStains.Components;

/// <summary>
/// Added to walls that currently have WallStain entities on them
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StainedWallComponent : Component
{
}
