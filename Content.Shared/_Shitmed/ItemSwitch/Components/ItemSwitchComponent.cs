// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.ItemSwitch.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ItemSwitchComponent : Component
{
    /// <summary>
    ///     The item's toggle state.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string State;

    [DataField(readOnly: true)]
    public Dictionary<string, ItemSwitchState> States = [];

    /// <summary>
    /// Can the entity be activated in the world.
    /// </summary>
    [DataField]
    public bool OnActivate = true;

    /// <summary>
    /// If this is set to false then the item can't be toggled by pressing Z.
    /// Use another system to do it then.
    /// </summary>
    [DataField]
    public bool OnUse = true;

    /// <summary>
    ///     Whether the item's toggle can be predicted by the client.
    /// </summary>
    /// /// <remarks>
    /// If server-side systems affect the item's toggle, like charge/fuel systems, then the item is not predictable.
    /// </remarks>
    [DataField]
    public bool Predictable = true;

    /// <summary>
    ///     Whether the item's currently toggled state should be shown in the UI.
    /// </summary>
    [DataField]
    public bool ShowLabel;

    /// <summary>
    ///     Whether the item requires power to sustain a state.
    /// </summary>
    [DataField]
    public bool NeedsPower;

    /// <summary>
    ///     Whether the item currently has enough power to sustain a state.
    /// </summary>
    [DataField]
    public bool IsPowered = true;

    /// <summary>
    ///     The default state of an item, which is also the state it reverts to when out of power.
    /// </summary>
    [DataField]
    public string? DefaultState;

    public ItemSwitchComponent(string state)
    {
        State = state;
    }
}
[DataDefinition]
public sealed partial class ItemSwitchState : BoundUserInterfaceMessage
{
    [DataField]
    public string? Verb;

    [DataField]
    public SoundSpecifier? SoundStateActivate;

    [DataField]
    public SoundSpecifier? SoundFailToActivate;

    [DataField]
    public ComponentRegistry? Components;

    [DataField]
    public bool RemoveComponents = true;

    [DataField]
    public SpriteSpecifier? Sprite;

    [DataField]
    public bool Hidden;

    /// <summary>
    ///     Amount of energy consumed per swing
    /// </summary>
    [DataField]
    public int EnergyPerUse;

    public ItemSwitchState(string verb)
    {
        Verb = verb;
    }
}

/// <summary>
/// Raised directed on an entity when its ItemToggle is attempted to be activated.
/// </summary>
[ByRefEvent]
public record struct ItemSwitchAttemptEvent()
{
    public bool Cancelled = false;
    public required EntityUid? User { get; init; }
    public required  string State { get; init; }
    /// <summary>
    /// Pop-up that gets shown to users explaining why the attempt was cancelled.
    /// </summary>
    public string? Popup { get; set; }
}

/// <summary>
/// Raised directed on an entity any sort of toggle is complete.
/// </summary>
[ByRefEvent]
public readonly record struct ItemSwitchedEvent
{
    public required bool Predicted { get; init; }
    public required string State { get; init; }
    public required EntityUid? User { get; init; }
}
