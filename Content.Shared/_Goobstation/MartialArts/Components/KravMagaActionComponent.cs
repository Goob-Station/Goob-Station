using Content.Shared.Actions;

namespace Content.Shared._Goobstation.MartialArts.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class KravMagaActionComponent : Component
{
    [DataField]
    public KravMagaMoves Configuration;
}

public enum KravMagaMoves
{
    LegSweep,
    NeckChop,
    LungPunch,
}

public sealed partial class KravMagaActionEvent : InstantActionEvent
{
}
