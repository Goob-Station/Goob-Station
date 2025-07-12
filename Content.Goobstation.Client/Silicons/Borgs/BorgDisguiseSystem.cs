// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Silicons.Borgs;
using Content.Goobstation.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Silicons.Borgs;

public sealed class BorgDisguiseSystem : SharedBorgDisguiseSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly BorgSystem _borgSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLightSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Silicons.Borgs.BorgDisguiseComponent, Shared.Silicons.Borgs.BorgDisguiseToggleActionEvent>(OnDisguiseToggle);
        SubscribeLocalEvent<Shared.Silicons.Borgs.BorgDisguiseComponent, AppearanceChangeEvent>(OnBorgAppearanceChanged);
    }

    /// <summary>
    /// Toggles the disguise.
    /// </summary>
    /// <param name="uid">The entity to toggle the disguise of.</param>
    /// <param name="comp">The disguise component of the entity.</param>
    /// <param name="args"></param>
    private void OnDisguiseToggle(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp, Shared.Silicons.Borgs.BorgDisguiseToggleActionEvent args)
    {
        UpdateAppearance(uid, comp);
        args.Handled = true;
    }

    /// <summary>
    /// Handles updates to the appearance of the entity.
    /// </summary>
    /// <param name="uid">The entity updated.</param>
    /// <param name="comp">The disguise component of the updated entity.</param>
    /// <param name="args"></param>
    private void OnBorgAppearanceChanged(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        UpdateAppearance(uid, comp);
    }

    /// <summary>
    /// Updates the appearance data of the entity.
    /// </summary>
    /// <param name="uid">The entity to update.</param>
    /// <param name="comp">The component holding the disguise data.</param>
    private void UpdateAppearance(EntityUid uid, Shared.Silicons.Borgs.BorgDisguiseComponent comp)
    {
        AppearanceComponent? appearance = null;
        SpriteComponent? sprite = null;

        if (!Resolve(uid, ref appearance, ref sprite))
            return;
        _appearance.SetData(uid, BorgDisguiseVisuals.IsDisguised, comp.Disguised, appearance);
        // Change method in BorgSystem gets automatically called via observer

        if (TryPrototype(uid, out var entityPrototype))
        {
            if (entityPrototype.TryGetComponent<BorgChassisComponent>("BorgChassis", out var borgPrototype))
            {
                _borgSystem.SetMindStates(new Entity<BorgChassisComponent>(uid, Comp<BorgChassisComponent>(uid)),
                    comp.Disguised ? comp.HasMindState : borgPrototype.HasMindState,
                    comp.Disguised ? comp.NoMindState : borgPrototype.NoMindState);
            }

            if (entityPrototype.TryGetComponent<PointLightComponent>("PointLight", out var lightPrototype))
            {
                _pointLightSystem.SetColor(uid,
                    comp.Disguised
                        ? comp.DisguisedLightColor
                        : lightPrototype.Color);
            }
        }


        sprite.LayerSetState("light", comp.Disguised ? comp.DisguisedLight : comp.RealLight);


        UpdateSharedAppearance(uid, comp);
    }
}
