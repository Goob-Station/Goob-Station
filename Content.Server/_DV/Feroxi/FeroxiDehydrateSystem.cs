// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 AstroDogeDX <48888500+AstroDogeDX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Nutrition.Components;

namespace Content.Server._DV.Feroxi;

public sealed class FeroxiDehydrateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;

    public override void Update(float frameTime) /// Goob start, reverted this section back to what it was on DV (Durks change broke things)
    {
        var query = EntityQueryEnumerator<FeroxiDehydrateComponent, ThirstComponent>();

        while (query.MoveNext(out var uid, out var feroxiDehydrate, out var thirst))
        {
            var currentThirst = thirst.CurrentThirst;
            var shouldBeDehydrated = currentThirst <= feroxiDehydrate.DehydrationThreshold;

            if (feroxiDehydrate.Dehydrated != shouldBeDehydrated)
            {
                UpdateDehydrationStatus((uid, feroxiDehydrate), shouldBeDehydrated);
            }
        }
    } /// Goob end

    /// <summary>
    /// Checks and changes the lungs when meeting the threshold for a swap of metabolizer
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="shouldBeDehydrated"></param>
    private void UpdateDehydrationStatus(Entity<FeroxiDehydrateComponent> ent, bool shouldBeDehydrated)
    {
        if (ent.Comp.Dehydrated == shouldBeDehydrated)
            return;

        ent.Comp.Dehydrated = shouldBeDehydrated;

        foreach (var entity in _body.GetBodyOrganEntityComps<LungComponent>(ent.Owner))
        {
            if (!TryComp<MetabolizerComponent>(entity, out var metabolizer) || metabolizer.MetabolizerTypes == null)
                continue;

            //Changing the metabolizer to the appropriate value based
            var newMetabolizer = shouldBeDehydrated ? ent.Comp.DehydratedMetabolizer : ent.Comp.HydratedMetabolizer;
            metabolizer.MetabolizerTypes!.Clear();
            metabolizer.MetabolizerTypes.Add(newMetabolizer);
        }
    }
}
