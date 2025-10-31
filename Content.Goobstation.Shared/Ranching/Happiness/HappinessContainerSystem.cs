using Content.Goobstation.Common.Ranching;
using Content.Goobstation.Shared.Ranching.Food;
using Content.Shared.Nutrition;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ranching.Happiness;

/// <summary>
/// This handles happiness.
/// </summary>
public sealed class HappinessContainerSystem : EntitySystem
{
    #region Public API

    [PublicAPI]
    public void AdjustHappiness(Entity<HappinessContainerComponent?> ent, int amount)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (amount > 0)
        {
            var maxDrain = 0;
            if (ent.Comp.MaxHappiness == -1)
                maxDrain = amount;
            else
                maxDrain = Math.Clamp(maxDrain + amount, 0, ent.Comp.MaxHappiness);

            ent.Comp.MaxHappiness += maxDrain;
            Dirty(ent);
            return;
        }

        ent.Comp.Happiness += amount;
        Dirty(ent);
    }

    [PublicAPI]
    public void SetHappiness(Entity<HappinessContainerComponent?> ent, int amount)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.Happiness = amount;
        Dirty(ent);
    }

    [PublicAPI]
    public int GetHappiness(Entity<HappinessContainerComponent?> ent, int amount)
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
