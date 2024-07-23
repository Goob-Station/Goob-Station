using Content.Shared.Actions;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Abilities;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HideUnderTableAbilityComponent : Component
{
    [DataField]
    public EntityUid? ToggleHideAction;


    [DataField("actionProto")]
    public EntProtoId? ActionProto;

    [DataField]
    public bool Enabled = false;

    /// <summary>
    ///     List of fixtures that had their collision mask changed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(string key, int originalMask)> ChangedFixtures = new();

    [DataField]
    public int? OriginalDrawDepth;
}

[Serializable, NetSerializable]
public enum SneakMode : byte
{
    Enabled
}

public sealed partial class ToggleHideUnderTablesEvent : InstantActionEvent
{ }

[Serializable, NetSerializable]
public sealed partial class HideUnderTablesUpdatedEvent : EventArgs
{
    public readonly bool Enabled = false;

    public HideUnderTablesUpdatedEvent(bool enabled)
    {
        Enabled = enabled;
    }
}