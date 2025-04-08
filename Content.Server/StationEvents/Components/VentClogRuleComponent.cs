// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.StationEvents.Events;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(VentClogRule))]
public sealed partial class VentClogRuleComponent : Component
{
    /// <summary>
    /// Somewhat safe chemicals to put in foam that probably won't instantly kill you.
    /// There is a small chance of using any reagent, ignoring this.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<ReagentPrototype>))]
    public IReadOnlyList<string> SafeishVentChemicals = new[]
    {
        "Water", "Blood", "Slime", "SpaceDrugs", "SpaceCleaner", "Nutriment", "Sugar", "SpaceLube", "Ephedrine", "Ale", "Beer", "SpaceGlue"
    };

    /// <summary>
    /// Sound played when foam is being created.
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/extinguish.ogg");

    /// <summary>
    /// The standard reagent quantity to put in the foam, modified by event severity.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int ReagentQuantity = 100;

    /// <summary>
    /// The standard spreading of the foam, not modified by event severity.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Spread = 16;

    /// <summary>
    /// How long the foam lasts for
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Time = 20f;

    /// <summary>
    /// Reagents that gets the weak numbers used instead of regular ones.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<ReagentPrototype>))]
    public IReadOnlyList<string> WeakReagents = new[]
    {
        "SpaceLube", "SpaceGlue"
    };

    /// <summary>
    /// Quantity of weak reagents to put in the foam.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int WeakReagentQuantity = 50;

    /// <summary>
    /// Spread of the foam for weak reagents.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int WeakSpread = 3;
}