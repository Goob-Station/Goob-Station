using Content.Goobstation.Common.Ranching;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Ranching.Food;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Happiness;

/// <summary>
/// This handles happiness.
/// </summary>
public sealed class HappinessContainerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HappinessContainerComponent, RanchingFoodEatenEvent>(OnRanchingFoodEaten);
    }

    private void OnRanchingFoodEaten(Entity<HappinessContainerComponent> ent, ref RanchingFoodEatenEvent args)
    {
        var preferences = GetPreferences(args.Food);
        foreach (var pref in preferences)
        {
            if (!ent.Comp.Preferences.TryGetValue(pref, out var prefValue))
                continue;

            AdjustHappiness(ent.Owner, prefValue);
        }
    }

    #region Public API
    [PublicAPI]
    public void AdjustHappiness(EntityUid uid, FixedPoint2 amount) =>
        AdjustHappiness(uid, amount, null, false);

    /// <summary>
    /// Adjusts the happiness for an entity
    /// </summary>
    /// <param name="ent"></param> The entity to adjust the happiness for
    /// <param name="amount"></param> How much to adjust the happiness
    /// <param name="cameFrom"></param> The entity that initiated the happiness change (e.g. a player who gave food to a chicken)
    /// <param name="naturalCause"></param> Was this a natural cause (e.g. happiness got adjusted by starving)
    [PublicAPI]
    public void AdjustHappiness(Entity<HappinessContainerComponent?> ent, FixedPoint2 amount, EntityUid? cameFrom, bool naturalCause)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        // this is so nested but istg i cant for the life of me understand dm code so idc anymore
        if (amount > 0)
        {
            if (!naturalCause)
            {
                // adjust visuals here
            }

            FixedPoint2 maximumDrain = 0;
            if (ent.Comp.MaxHappiness == -1)
                maximumDrain = amount;
            else
            {
                if (ent.Comp.MaxHappiness == 0)
                    return;
                maximumDrain = FixedPoint2.Min(ent.Comp.MaxHappiness, amount);
            }

            ent.Comp.MaxHappiness -= maximumDrain;
            ent.Comp.Happiness += maximumDrain;
        }
        else
        {
            if (!naturalCause)
            {
                // adjust visuals here
            }

            ent.Comp.Happiness += amount;
        }

        if (cameFrom.HasValue)
        {
            // adjust friendship here
        }

        Dirty(ent);
    }

    [PublicAPI]
    public void SetHappiness(Entity<HappinessContainerComponent?> ent, FixedPoint2 amount)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.Happiness = amount;
        Dirty(ent);
    }

    [PublicAPI]
    public FixedPoint2 GetHappiness(Entity<HappinessContainerComponent?> ent, FixedPoint2 amount)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return 0;

        return ent.Comp.Happiness;
    }

    [PublicAPI]
    public IReadOnlyList<ProtoId<HappinessPreferencePrototype>> GetPreferences(Entity<PreferencesHolderComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return Array.Empty<ProtoId<HappinessPreferencePrototype>>();

        return ent.Comp.Preferences;
    }

    #endregion
}
