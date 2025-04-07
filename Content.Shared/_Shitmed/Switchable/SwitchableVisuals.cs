using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Switchable;

public sealed partial class SwitchableActionEvent : InstantActionEvent;

/// <summary>
///     Generic enum keys for toggle-visualizer appearance data & sprite layers.
/// </summary>
[Serializable, NetSerializable]
public enum SwitchableVisuals : byte
{
    Switched,
    Layer
}