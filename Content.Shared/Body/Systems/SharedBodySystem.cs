// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

// Shitmed Change
using Content.Shared.Inventory;
using Robust.Shared.Random;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBodySystem : EntitySystem
{
    /*
     * See the body partial for how this works.
     */

    /// <summary>
    /// Container ID prefix for any body parts.
    /// </summary>
    public const string PartSlotContainerIdPrefix = "body_part_slot_";

    /// <summary>
    /// Container ID for the ContainerSlot on the body entity itself.
    /// </summary>
    public const string BodyRootContainerId = "body_root_part";

    /// <summary>
    /// Container ID prefix for any body organs.
    /// </summary>
    public const string OrganSlotContainerIdPrefix = "body_organ_slot_";

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly IPrototypeManager Prototypes = default!;
    [Dependency] protected readonly DamageableSystem Damageable = default!;
    [Dependency] protected readonly MovementSpeedModifierSystem Movement = default!;
    [Dependency] protected readonly SharedContainerSystem Containers = default!;
    [Dependency] protected readonly SharedTransformSystem SharedTransform = default!;
    [Dependency] protected readonly StandingStateSystem Standing = default!;
    [Dependency] private readonly IRobustRandom _random = default!; // Shitmed Change
    [Dependency] private readonly InventorySystem _inventory = default!; // Shitmed Change

    public override void Initialize()
    {
        base.Initialize();

        InitializeBody();
        InitializeParts();
        InitializeOrgans();
        // Shitmed Change Start
        InitializePartAppearances();
        InitializeRelay();
        // Shitmed Change End
    }

    /// <summary>
    /// Inverse of <see cref="GetPartSlotContainerId"/>
    /// </summary>
    protected static string? GetPartSlotContainerIdFromContainer(string containerSlotId)
    {
        // This is blursed
        var slotIndex = containerSlotId.IndexOf(PartSlotContainerIdPrefix, StringComparison.Ordinal);

        if (slotIndex < 0)
            return null;

        var slotId = containerSlotId.Remove(slotIndex, PartSlotContainerIdPrefix.Length);
        return slotId;
    }

    /// <summary>
    /// Gets the container Id for the specified slotId.
    /// </summary>
    public static string GetPartSlotContainerId(string slotId)
    {
        return PartSlotContainerIdPrefix + slotId;
    }

    /// <summary>
    /// Gets the container Id for the specified slotId.
    /// </summary>
    public static string GetOrganContainerId(string slotId)
    {
        return OrganSlotContainerIdPrefix + slotId;
    }
}