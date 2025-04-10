using Content.Shared.Access.Systems;
using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Access.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(IdExaminableSystem))]
public sealed partial class IdExaminableComponent : Component
{
    public bool WantedVerbVisible;
    [DataField]
    public ProtoId<RadioChannelPrototype> SecurityChannel = "Security";
    [DataField]
    public uint MaxStringLength = 256;
}
[Serializable, NetSerializable]
public sealed class RefreshVerbsEvent : EntityEventArgs
{
    public readonly NetEntity Target;
    public RefreshVerbsEvent(NetEntity target)
    {
        Target = target;
    }
}
[Serializable, NetSerializable]
public sealed class ResetWantedVerbEvent : EntityEventArgs
{
    public readonly NetEntity Target;
    public ResetWantedVerbEvent(NetEntity target)
    {
        Target = target;
    }
}

[NetSerializable, Serializable]
public enum SetWantedVerbMenu : byte
{
    Key,
}

public record struct OpenWantedUiEvent(string Name, EntityUid Target);