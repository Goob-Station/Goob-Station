// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

/*
    2026-05-27
    This is currently in Common as a few modified downstream files check for an AbductorScientistComponent.
    If that is changed. then this file and AbductorEnums can go back into shared, and CommonAbductorSystem
    be deleted entirely in lieu of the existing SharedAbductorSystem.
*/

using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Shitmed.Antags.Abductor;

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorHumanObservationConsoleComponent : Component
{
    [DataField(readOnly: true)]
    public EntProtoId? RemoteEntityProto = "AbductorHumanObservationConsoleEye";

    [DataField, AutoNetworkedField]
    public NetEntity? RemoteEntity;
}
[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public NetEntity? Target;

    [DataField, AutoNetworkedField]
    public NetEntity? AlienPod;

    [DataField, AutoNetworkedField]
    public NetEntity? Experimentator;

    [DataField, AutoNetworkedField]
    public NetEntity? Armor;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem))]
public sealed partial class AbductorAlienPadComponent : Component
{
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorExperimentatorComponent : Component
{
    [DataField, AutoNetworkedField]
    public NetEntity? Console;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string ContainerId = "storage";
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorGizmoComponent : Component
{
    [DataField, AutoNetworkedField]
    public NetEntity? Target;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem))]
public sealed partial class AbductorComponent : Component
{
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AbductorVictimComponent : Component
{
    [DataField("position"), AutoNetworkedField]
    public EntityCoordinates? Position;

    [DataField, AutoNetworkedField]
    public bool Implanted;

    [DataField, AutoNetworkedField]
    public TimeSpan? LastActivation;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem))]
public sealed partial class AbductorOrganComponent : Component;

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorScientistComponent : Component
{
    [DataField("position"), AutoNetworkedField]
    public EntityCoordinates? SpawnPosition;

    [DataField, AutoNetworkedField]
    public EntityUid? Console;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class RemoteEyeSourceContainerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Actor;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorsAbilitiesComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? ExitConsole;

    [DataField, AutoNetworkedField]
    public EntityUid? SendYourself;

    [DataField]
    public EntityUid[] HiddenActions = [];
}

[RegisterComponent, NetworkedComponent, Access(typeof(CommonAbductorSystem)), AutoGenerateComponentState]
public sealed partial class AbductorVestComponent : Component
{
    [DataField, AutoNetworkedField]
    public AbductorArmorModeType CurrentState = AbductorArmorModeType.Stealth;
}
[RegisterComponent, Access(typeof(CommonAbductorSystem))]
public sealed partial class AbductConditionComponent : Component
{
    [DataField("abducted"), ViewVariables(VVAccess.ReadWrite)]
    public int Abducted;
    [DataField("hashset"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<NetEntity> AbductedHashs = [];
}
