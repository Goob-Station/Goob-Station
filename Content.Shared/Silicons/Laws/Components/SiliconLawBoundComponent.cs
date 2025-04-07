// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.Laws.Components;

/// <summary>
/// This is used for entities which are bound to silicon laws and can view them.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSiliconLawSystem))]
public sealed partial class SiliconLawBoundComponent : Component
{
    /// <summary>
    /// The last entity that provided laws to this entity.
    /// </summary>
    [DataField]
    public EntityUid? LastLawProvider;
}

/// <summary>
/// Event raised to get the laws that a law-bound entity has.
///
/// Is first raised on the entity itself, then on the
/// entity's station, then on the entity's grid,
/// before being broadcast.
/// </summary>
/// <param name="Entity"></param>
[ByRefEvent]
public record struct GetSiliconLawsEvent(EntityUid Entity)
{
    public EntityUid Entity = Entity;

    public SiliconLawset Laws = new();

    public bool Handled = false;
}

public sealed partial class ToggleLawsScreenEvent : InstantActionEvent
{

}

[NetSerializable, Serializable]
public enum SiliconLawsUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class SiliconLawBuiState : BoundUserInterfaceState
{
    public List<SiliconLaw> Laws;
    public HashSet<string>? RadioChannels;

    public SiliconLawBuiState(List<SiliconLaw> laws, HashSet<string>? radioChannels)
    {
        Laws = laws;
        RadioChannels = radioChannels;
    }
}