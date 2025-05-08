// SPDX-FileCopyrightText: 2018 PJB3005 <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2019 ZelteHonor <gabrieldionbouchard@gmail.com>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Matt <matt@isnor.io>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vyacheslav Titov <rincew1nd@ya.ru>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 vanx <61917534+Vaaankas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 vanx <vanx#5477>
// SPDX-FileCopyrightText: 2023 Титов Вячеслав Витальевич <rincew1nd@yandex.ru>
// SPDX-FileCopyrightText: 2024 Vasilis <vascreeper@yahoo.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.GameTicking;
using Content.Shared.Mind.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mind;

/// <summary>
///     This component stores information about a player/mob mind. The component will be attached to a mind-entity
///     which is stored in null-space. The entity that is currently "possessed" by the mind will have a
///     <see cref="MindContainerComponent"/>.
/// </summary>
/// <remarks>
///     Roles are attached as components on the mind-entity entity.
///     Think of it like this: if a player is supposed to have their memories,
///     their mind follows along.
///
///     Things such as respawning do not follow, because you're a new character.
///     Getting borged, cloned, turned into a catbeast, etc... will keep it following you.
///
///     Minds are stored in null-space, and are thus generally not set to players unless that player is the owner
///     of the mind. As a result it should be safe to network "secret" information like roles & objectives
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MindComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Objectives = new();

    /// <summary>
    ///     The session ID of the player owning this mind.
    /// </summary>
    [DataField, AutoNetworkedField, Access(typeof(SharedMindSystem))]
    public NetUserId? UserId { get; set; }

    /// <summary>
    ///     The session ID of the original owner, if any.
    ///     May end up used for round-end information (as the owner may have abandoned Mind since)
    /// </summary>
    [DataField, AutoNetworkedField, Access(typeof(SharedMindSystem))]
    public NetUserId? OriginalOwnerUserId { get; set; }

    /// <summary>
    ///     The first entity that this mind controlled. Used for round end information.
    ///     Might be relevant if the player has ghosted since.
    /// </summary>
    [AutoNetworkedField]
    public NetEntity? OriginalOwnedEntity; // TODO WeakEntityReference make this a Datafield again
    // This is a net entity, because this field currently does not get set to null when this entity is deleted.
    // This is a lazy way to ensure that people check that the entity still exists.
    // TODO MIND Fix this properly by adding an OriginalMindContainerComponent or something like that.

    [ViewVariables]
    public bool IsVisitingEntity => VisitingEntity != null;

    [DataField, AutoNetworkedField, Access(typeof(SharedMindSystem))]
    public EntityUid? VisitingEntity { get; set; }

    [ViewVariables]
    public EntityUid? CurrentEntity => VisitingEntity ?? OwnedEntity;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public string? CharacterName { get; set; }

    /// <summary>
    ///     The time of death for this Mind.
    ///     Can be null - will be null if the Mind is not considered "dead".
    /// </summary>
    [DataField]
    public TimeSpan? TimeOfDeath { get; set; }

    /// <summary>
    ///     The entity currently owned by this mind.
    ///     Can be null.
    /// </summary>
    [DataField, AutoNetworkedField, Access(typeof(SharedMindSystem))]
    public EntityUid? OwnedEntity { get; set; }

    /// <summary>
    ///     An enumerable over all the objective entities this mind has.
    /// </summary>
    [ViewVariables, Obsolete("Use Objectives field")]
    public IEnumerable<EntityUid> AllObjectives => Objectives;

    /// <summary>
    ///     Prevents user from ghosting out
    /// </summary>
    [DataField]
    public bool PreventGhosting { get; set; }

    /// <summary>
    ///     Prevents user from suiciding
    /// </summary>
    [DataField]
    public bool PreventSuicide { get; set; }

    /// <summary>
    ///     Mind Role Entities belonging to this Mind
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> MindRoles = new List<EntityUid>();

    /// <summary>
    ///     The mind's current antagonist/special role, or lack thereof;
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<RoleTypePrototype> RoleType = "Neutral";

    /// <summary>
    ///     The session of the player owning this mind.
    ///     Can be null, in which case the player is currently not logged in.
    /// </summary>
    [ViewVariables, Access(typeof(SharedMindSystem), typeof(SharedGameTicker))]
    // TODO remove this after moving IPlayerManager functions to shared
    public ICommonSession? Session { get; set; }
}