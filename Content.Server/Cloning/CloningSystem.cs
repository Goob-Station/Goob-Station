// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 SoulSloth <67545203+SoulSloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
// SPDX-FileCopyrightText: 2022 och-och <80923370+och-och@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 corentt <36075110+corentt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Cloning.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.EUI;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Humanoid;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Cloning;
using Content.Shared.Cloning.Events;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.NameModifier.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.Cloning;

/// <summary>
///     System responsible for making a copy of a humanoid's body.
///     For the cloning machines themselves look at CloningPodSystem, CloningConsoleSystem and MedicalScannerSystem instead.
/// </summary>
public sealed class CloningSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    /// <summary>
    ///     Spawns a clone of the given humanoid mob at the specified location or in nullspace.
    /// </summary>
    public bool TryCloning(EntityUid original, MapCoordinates? coords, ProtoId<CloningSettingsPrototype> settingsId, [NotNullWhen(true)] out EntityUid? clone)
    {
        clone = null;
        if (!_prototype.TryIndex(settingsId, out var settings))
            return false; // invalid settings

        if (!TryComp<HumanoidAppearanceComponent>(original, out var humanoid))
            return false; // whatever body was to be cloned, was not a humanoid

        if (!_prototype.TryIndex(humanoid.Species, out var speciesPrototype))
            return false; // invalid species

        if (HasComp<SiliconComponent>(original))
            return false; // Goobstation: Don't clone IPCs. It could be argued it should be in the CloningPodSystem instead

        var attemptEv = new CloningAttemptEvent(settings);
        RaiseLocalEvent(original, ref attemptEv);
        if (attemptEv.Cancelled && !settings.ForceCloning)
            return false; // cannot clone, for example due to the unrevivable trait

        clone = coords == null ? Spawn(speciesPrototype.Prototype) : Spawn(speciesPrototype.Prototype, coords.Value);
        _humanoidSystem.CloneAppearance(original, clone.Value);

        var componentsToCopy = settings.Components;

        // don't make status effects permanent
        if (TryComp<StatusEffectsComponent>(original, out var statusComp))
            componentsToCopy.ExceptWith(statusComp.ActiveEffects.Values.Select(s => s.RelevantComponent).Where(s => s != null)!);

        foreach (var componentName in componentsToCopy)
        {
            if (!_componentFactory.TryGetRegistration(componentName, out var componentRegistration))
            {
                Log.Error($"Tried to use invalid component registration for cloning: {componentName}");
                continue;
            }

            if (EntityManager.TryGetComponent(original, componentRegistration.Type, out var sourceComp)) // Does the original have this component?
            {
                if (HasComp(clone.Value, componentRegistration.Type)) // CopyComp cannot overwrite existing components
                    RemComp(clone.Value, componentRegistration.Type);
                CopyComp(original, clone.Value, sourceComp);
            }
        }

        var cloningEv = new CloningEvent(settings, clone.Value);
        RaiseLocalEvent(original, ref cloningEv); // used for datafields that cannot be directly copied

        // Add equipment first so that SetEntityName also renames the ID card.
        if (settings.CopyEquipment != null)
            CopyEquipment(original, clone.Value, settings.CopyEquipment.Value, settings.Whitelist, settings.Blacklist);

        var originalName = Name(original);
        if (TryComp<NameModifierComponent>(original, out var nameModComp)) // if the originals name was modified, use the unmodified name
            originalName = nameModComp.BaseName;

        // This will properly set the BaseName and EntityName for the clone.
        // Adding the component first before renaming will make sure RefreshNameModifers is called.
        // Without this the name would get reverted to Urist.
        // If the clone has no name modifiers, NameModifierComponent will be removed again.
        EnsureComp<NameModifierComponent>(clone.Value);
        _metaData.SetEntityName(clone.Value, originalName);

        _adminLogger.Add(LogType.Chat, LogImpact.Medium, $"The body of {original:player} was cloned as {clone.Value:player}");
        return true;
    }

    /// <summary>
    ///     Copies the equipment the original has to the clone.
    ///     This uses the original prototype of the items, so any changes to components that are done after spawning are lost!
    /// </summary>
    public void CopyEquipment(EntityUid original, EntityUid clone, SlotFlags slotFlags, EntityWhitelist? whitelist = null, EntityWhitelist? blacklist = null)
    {
        if (!TryComp<InventoryComponent>(original, out var originalInventory) || !TryComp<InventoryComponent>(clone, out var cloneInventory))
            return;
        // Iterate over all inventory slots
        var slotEnumerator = _inventory.GetSlotEnumerator((original, originalInventory), slotFlags);
        while (slotEnumerator.NextItem(out var item, out var slot))
        {
            // Spawn a copy of the item using the original prototype.
            // This means any changes done to the item after spawning will be reset, but that should not be a problem for simple items like clothing etc.
            // we use a whitelist and blacklist to be sure to exclude any problematic entities

            if (_whitelist.IsWhitelistFail(whitelist, item) || _whitelist.IsBlacklistPass(blacklist, item))
                continue;

            var prototype = MetaData(item).EntityPrototype;
            if (prototype != null)
                _inventory.SpawnItemInSlot(clone, slot.Name, prototype.ID, silent: true, inventory: cloneInventory);
        }
    }
}