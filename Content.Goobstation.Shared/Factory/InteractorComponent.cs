// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedInteractorSystem))]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class InteractorComponent : Component
{
    #region Linking
    /// <summary>
    /// Sink port that starts interacting with some target when invoked.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> StartPort = "InteractorStart";

    [DataField]
    public ProtoId<SourcePortPrototype> StartedPort = "InteractorStarted";

    [DataField]
    public ProtoId<SourcePortPrototype> CompletedPort = "InteractorCompleted";

    [DataField]
    public ProtoId<SourcePortPrototype> FailedPort = "InteractorFailed";
    #endregion

    #region Target Items
    /// <summary>
    /// Fixture to look for target items with.
    /// </summary>
    [DataField]
    public string TargetFixtureId = "interactor_target";

    /// <summary>
    /// Entities currently colliding with <see cref="TargetFixtureId"/> and whether their CollisionWake was enabled.
    /// When entities start to collide they get pushed to the end.
    /// When picking up items the last value is taken.
    /// This is essentially a FILO queue.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(NetEntity, bool)> TargetEntities = new();
    #endregion
}
