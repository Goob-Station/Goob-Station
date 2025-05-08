using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.Targeting;

[Serializable, NetSerializable]
public sealed class SetSwapSecondaryTarget(NetEntity action, NetEntity? target) : EntityEventArgs
{
    public NetEntity Action = action;

    public NetEntity? Target = target;
}
