// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Machine that starts constructions.
/// Multi-step objects will need interactors to complete their steps.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedConstructorSystem))]
[AutoGenerateComponentState]
public sealed partial class ConstructorComponent : Component
{
    /// <summary>
    /// The construction it will try to build when start is invoked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<ConstructionPrototype>? Construction;
}

[Serializable, NetSerializable]
public enum ConstructorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ConstructorSetProtoMessage(ProtoId<ConstructionPrototype>? id) : BoundUserInterfaceMessage
{
    public ProtoId<ConstructionPrototype>? Id = id;
}
