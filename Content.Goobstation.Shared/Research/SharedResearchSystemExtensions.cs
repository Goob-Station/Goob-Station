using Content.Shared.Lathe;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Content.Shared.Research.Systems;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Shared.Research;

public static class SharedResearchSystemExtensions
{
    public static int GetTierCompletionPercentage(this SharedResearchSystem system,
        TechnologyDatabaseComponent component,
        TechDisciplinePrototype techDiscipline,
        IPrototypeManager prototypeManager)
    {
        var allTech = prototypeManager.EnumeratePrototypes<TechnologyPrototype>()
            .Where(p => p.Discipline == techDiscipline.ID && !p.Hidden).ToList();

        var percentage = (float) component.UnlockedTechnologies
            .Where(x => prototypeManager.Index<TechnologyPrototype>(x).Discipline == techDiscipline.ID)
            .Count() / (float) allTech.Count * 100f;

        return (int) Math.Clamp(percentage, 0, 100);
    }
}
