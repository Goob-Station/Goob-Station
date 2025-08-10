using Robust.Shared.GameStates;

namespace Content.Shared._DV.Implants.Components;

/// <summary>
/// Implant to get MimePowers status (to summon walls and take the mime's vow) for characters who are canonically mute
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MimePowersImplantComponent : Component;
