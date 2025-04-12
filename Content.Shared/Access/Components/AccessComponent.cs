// SPDX-FileCopyrightText: 2019 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 FL-OZ <58238103+FL-OZ@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 FL-OZ <anotherscuffed@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Access.Components;

/// <summary>
///     Simple mutable access provider found on ID cards and such.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedAccessSystem))]
[AutoGenerateComponentState]
public sealed partial class AccessComponent : Component
{
    /// <summary>
    /// True if the access provider is enabled and can grant access.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool Enabled = true;

    [DataField]
    [Access(typeof(SharedAccessSystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
    [AutoNetworkedField]
    public HashSet<ProtoId<AccessLevelPrototype>> Tags = new();

    /// <summary>
    /// Access Groups. These are added to the tags during map init. After map init this will have no effect.
    /// </summary>
    [DataField(readOnly: true)]
    [AutoNetworkedField]
    public HashSet<ProtoId<AccessGroupPrototype>> Groups = new();
}

/// <summary>
/// Event raised on an entity to find additional entities which provide access.
/// </summary>
[ByRefEvent]
public struct GetAdditionalAccessEvent
{
    public HashSet<EntityUid> Entities = new();

    public GetAdditionalAccessEvent()
    {
    }
}

[ByRefEvent]
public record struct GetAccessTagsEvent(HashSet<ProtoId<AccessLevelPrototype>> Tags, IPrototypeManager PrototypeManager)
{
    public void AddGroup(ProtoId<AccessGroupPrototype> group)
    {
        if (!PrototypeManager.TryIndex<AccessGroupPrototype>(group, out var groupPrototype))
            return;

        Tags.UnionWith(groupPrototype.Tags);
    }
}