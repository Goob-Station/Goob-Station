using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._MACRO.StrangeMoods.Eui;

[Serializable, NetSerializable]
public sealed class StrangeMoodsInitEuiState(NetEntity target) : EuiStateBase
{
    public NetEntity Target { get; } = target;
}

[Serializable, NetSerializable]
public sealed class StrangeMoodsInitAcceptMessage(StrangeMoodDefinition definition, NetEntity target) : EuiMessageBase
{
    public StrangeMoodDefinition Definition { get; } = definition;
    public NetEntity Target { get; } = target;
}