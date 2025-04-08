// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Tag; // Shitmed Change - Starlight Abductors

namespace Content.Server.Wires;

[RegisterComponent]
public sealed partial class WiresComponent : Component
{
    /// <summary>
    ///     The name of this entity's internal board.
    /// </summary>
    [DataField]
    public LocId BoardName { get; set; } = "wires-board-name-default";

    /// <summary>
    ///     The layout ID of this entity's wires.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<WireLayoutPrototype> LayoutId { get; set; } = default!;

    /// <summary>
    ///     The serial number of this board. Randomly generated upon start,
    ///     does not need to be set.
    /// </summary>
    [ViewVariables]
    public string? SerialNumber { get; set; }

    /// <summary>
    ///     The seed that dictates the wires appearance, as well as
    ///     the status ordering on the UI client side.
    /// </summary>
    [ViewVariables]
    public int WireSeed { get; set; }

    /// <summary>
    ///     The list of wires currently active on this entity.
    /// </summary>
    [ViewVariables]
    public List<Wire> WiresList { get; set; } = new();

    /// <summary>
    ///     Queue of wires saved while the wire's DoAfter event occurs, to prevent too much spam.
    /// </summary>
    [ViewVariables]
    public List<int> WiresQueue { get; } = new();

    /// <summary>
    ///     If this should follow the layout saved the first time the layout dictated by the
    ///     layout ID is generated, or if a new wire order should be generated every time.
    /// </summary>
    [DataField]
    public bool AlwaysRandomize { get; private set; }

    /// <summary>
    ///     Per wire status, keyed by an object.
    /// </summary>
    [ViewVariables]
    public Dictionary<object, object> Statuses { get; } = new();

    /// <summary>
    ///     The state data for the set of wires inside of this entity.
    ///     This is so that wire objects can be flyweighted between
    ///     entities without any issues.
    /// </summary>
    [ViewVariables]
    public Dictionary<object, object> StateData { get; } = new();

    [DataField]
    public SoundSpecifier PulseSound = new SoundPathSpecifier("/Audio/Effects/multitool_pulse.ogg");

    // Shitmed Change - Starlight Abductors
    [ViewVariables]
    public bool ViewWires = false;

    [DataField]
    public ProtoId<TagPrototype> ShowWiresTag = "ShowWires";
    // Shitmed Change End
}