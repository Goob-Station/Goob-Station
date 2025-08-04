// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission

using Robust.Shared.Serialization;

namespace Content.Shared._Mono.FireControl;

/// <summary>
/// Event sent from server to client to visualize firing directions for a weapon
/// </summary>
[Serializable, NetSerializable]
public sealed class FireControlVisualizationEvent : EntityEventArgs
{
    /// <summary>
    /// Entity to visualize
    /// </summary>
    public NetEntity Entity { get; }
    
    /// <summary>
    /// Dictionary mapping direction angles (in degrees) to whether firing is possible
    /// </summary>
    public Dictionary<float, bool>? Directions { get; }
    
    /// <summary>
    /// Whether to enable (true) or disable (false) visualization
    /// </summary>
    public bool Enabled { get; }
    
    /// <summary>
    /// Constructor for enabling/updating visualization with data
    /// </summary>
    public FireControlVisualizationEvent(NetEntity entity, Dictionary<float, bool> directions)
    {
        Entity = entity;
        Directions = directions;
        Enabled = true;
    }
    
    /// <summary>
    /// Constructor for toggling visualization off
    /// </summary>
    public FireControlVisualizationEvent(NetEntity entity)
    {
        Entity = entity;
        Directions = null;
        Enabled = false;
    }
} 