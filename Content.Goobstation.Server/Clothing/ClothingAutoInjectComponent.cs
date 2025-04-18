// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Clothing;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Clothing;
/// <summary>
/// Component applied by clothing that allows the wearer to inject themselves with a reagent on a cooldown.
/// Used for auto-injection mechanisms like emergency epi-pens or stimulants. Possible uses for a modsuit in the future.
/// </summary>
[RegisterComponent]
public sealed partial class ClothingAutoInjectComponent : Component
{
    /// <summary>
    /// The YAML prototype defining which chemicals and how much to inject.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<AutoInjectorPrototype> Proto;

    /// <summary>
    /// The ID of the action used to activate the auto-injector.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionActivateAutoinjector";

    /// <summary>
    /// The UID of the action, stored here so it can be safely removed.
    /// </summary>
    [DataField]
    public EntityUid? ActionEntity;
}
