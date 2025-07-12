// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicons.Borgs;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Silicons.Borgs;

public sealed class BorgDisguiseSystem : SharedBorgDisguiseSystem
{
    [Dependency] private readonly SharedPointLightSystem _pointLightSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Silicons.Borgs.BorgDisguiseComponent, Shared.Silicons.Borgs.BorgDisguiseToggleActionEvent>(OnDisguiseToggle);
        SubscribeLocalEvent<Shared.Silicons.Borgs.BorgDisguiseComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<Shared.Silicons.Borgs.BorgDisguiseComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    /// <summary>
    /// Toggles the disguise.
    /// </summary>
    /// <param name="uid">The entity to toggle the disguise of.</param>
    /// <param name="comp">The disguise component of the entity.</param>
    /// <param name="args"></param>
    private void OnDisguiseToggle(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp, Shared.Silicons.Borgs.BorgDisguiseToggleActionEvent args)
    {
        if (args.Handled)
            return;
        comp.Disguised = !comp.Disguised;
        Dirty(uid, comp);
        args.Handled = true;
        UpdateApperance(uid, comp);
    }

    /// <summary>
    /// Disables the disguise.
    /// </summary>
    /// <param name="uid">The entity having their disguise disabled.</param>
    /// <param name="comp">The disguise component being disabled.</param>
    private void DisableDisguise(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp)
    {
        comp.Disguised = false;
        Dirty(uid, comp);
        UpdateApperance(uid, comp);
    }

    /// <summary>
    /// Disables the disguise if the borg is no longer powered.
    /// </summary>
    /// <param name="uid">The entity to check</param>
    /// <param name="comp">The disguise component.</param>
    /// <param name="args">State change event.</param>
    private void OnToggled(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp, ref ItemToggledEvent args)
    {
        if (!args.Activated)
        {
            DisableDisguise(uid, comp);
        }
    }

    /// <summary>
    /// Disables the disguise if the borg is no longer alive.
    /// </summary>
    /// <param name="uid">The entity to check</param>
    /// <param name="component">The disguise component.</param>
    /// <param name="args">State change event.</param>
    private void OnMobStateChanged(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
        {
            DisableDisguise(uid, component);
        }
    }

    /// <summary>
    /// Updates the appearance data of the entity.
    /// </summary>
    /// <param name="uid">The entity to update.</param>
    /// <param name="comp">The component holding the disguise data.</param>
    private void UpdateApperance(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp)
    {
        if (TryPrototype(uid, out var entityPrototype))
        {
            if (entityPrototype.TryGetComponent<PointLightComponent>("PointLight", out var lightPrototype))
            {
                _pointLightSystem.SetColor(uid,
                    comp.Disguised
                        ? comp.DisguisedLightColor
                        : lightPrototype.Color);
            }
        }

        UpdateSharedAppearance(uid, comp);
    }
}
