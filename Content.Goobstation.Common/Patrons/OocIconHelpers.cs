using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Patrons;

public static class OocIconHelpers
{
    public static bool CanUse(this PatronTierPrototype tier, OocIconPrototype icon)
    {
        return tier.OocIconTiers.Contains(icon.Tier);
    }

    public static IEnumerable<OocIconPrototype> GetAvailableOocIcons(this IPrototypeManager prototypes, PatronTierPrototype tier)
    {
        foreach (var icon in prototypes.EnumeratePrototypes<OocIconPrototype>())
        {
            if (tier.CanUse(icon))
                yield return icon;
        }
    }

    public static PatronTierPrototype? GetPatronTierByDatabaseId(this IPrototypeManager prototypes, int databaseId)
    {
        foreach (var tier in prototypes.EnumeratePrototypes<PatronTierPrototype>())
        {
            if (tier.DatabaseId == databaseId)
                return tier;
        }

        return null;
    }
}
