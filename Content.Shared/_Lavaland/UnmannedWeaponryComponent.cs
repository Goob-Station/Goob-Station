using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Components;

/// <summary>
/// Marker component for shuttle weaponry to prevent cheesing hierophant.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UnmannedWeaponryComponent : Component;
