// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Assmos - Extinguisher Nozzle

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Atmos.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FirefighterTankRefillableComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    ///     Name of solution/>.
    /// </summary>
    [DataField]
    public const string SolutionName = "tank";

    /// <summary>
    ///     Reagent that will be used in backpack.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> TankReagent = "Water";

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public SlotFlags TargetSlot;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntityWhitelist? ProviderWhitelist;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ExternalContainer = false;

    /// <summary>
    ///     Sound played when refilling the backpack.
    /// </summary>
    [DataField]
    public SoundSpecifier FirefightingNozzleRefill = new SoundPathSpecifier("/Audio/Effects/refill.ogg");
}
