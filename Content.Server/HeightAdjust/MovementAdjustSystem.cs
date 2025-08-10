// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Content.Shared.HeightAdjust;
using Robust.Shared.Configuration;
using Content.Goobstation.Shared.Sprinting;

namespace Content.Server.HeightAdjust;

public sealed class MovementAdjustSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SprintingAffectedByScaleComponent, HeightAdjustedEvent>((uid, comp, args) => TryAdjustSprintSpeed((uid, comp), ref args));
    }

    /// <summary>
    ///     Adjusts the sprinting speed of the specified entity based on the settings provided by the component.
    /// </summary>
    public bool TryAdjustSprintSpeed(Entity<SprintingAffectedByScaleComponent> ent, ref HeightAdjustedEvent args)
    {
        if (!TryComp<SprinterComponent>(ent, out var sprinting)
            || sprinting.SprintSpeedMultiplier == 0f
            || !_config.GetCVar(CCVars.HeightAdjustModifiesSprinting)
            // HeightAdjustedEvent is called twice: once during initialization, to set the player to their species default scale, and once after initialization, to give the player their preferred scale.
            // We only care about the scale that the player selects. Otherwise, the sprint speed adjustment is applied twice, which would alter sprint speed more than intended. So, we check that the entity has been initialized.
            || !TryComp(ent, out MetaDataComponent? metaData)
            || !metaData.EntityInitialized
            )
            return false;

        args.NewScale.Deconstruct(out float xscale, out float yscale);

        var factor = (xscale + yscale) / 2;
        factor = Math.Clamp(factor, ent.Comp.Min, ent.Comp.Max);

        var newSprintSpeedMultiplier = sprinting.SprintSpeedMultiplier * factor;

        sprinting.SprintSpeedMultiplier = newSprintSpeedMultiplier;

        return true;
    }
}
