using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Nutrition.Components;

namespace Content.Server._DV.Feroxi;

public sealed class FeroxiDehydrateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;

    public override void Update(float frameTime)
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
    }

    /// <summary>
    /// Checks and changes the lungs when meeting the threshold for a swap of metabolizer
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="shouldBeDehydrated"></param>
    private void UpdateDehydrationStatus(Entity<FeroxiDehydrateComponent> ent, bool shouldBeDehydrated)
    {
        ent.Comp.Dehydrated = shouldBeDehydrated;

        foreach (var entity in _body.GetBodyOrganEntityComps<LungComponent>(ent.Owner))
        {
            if (!TryComp<MetabolizerComponent>(entity, out var metabolizer) || metabolizer.MetabolizerTypes == null)
            {
                continue;
            }
            //Changing the metabolizer to the appropriate value based
            var newMetabolizer = shouldBeDehydrated ? ent.Comp.DehydratedMetabolizer : ent.Comp.HydratedMetabolizer;
            metabolizer.MetabolizerTypes!.Clear();
            metabolizer.MetabolizerTypes.Add(newMetabolizer);
        }
    }
}
