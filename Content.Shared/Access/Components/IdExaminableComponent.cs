// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Systems;
using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Access.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(IdExaminableSystem))]
public sealed partial class IdExaminableComponent : Component
{
    [DataField]
    public ProtoId<RadioChannelPrototype> SecurityChannel = "Security";

    [DataField]
    public uint MaxStringLength = 256;
}

[NetSerializable, Serializable]
public enum SetWantedVerbMenu : byte
{
    Key,
}
