using System.Linq;
using Content.Shared.Lathe;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Research.Systems;

public abstract partial class SharedResearchSystem : EntitySystem
{
    public int GetTierCompletionPercentage(TechnologyDatabaseComponent component, TechDisciplinePrototype techDiscipline)
    {
        var allTech = PrototypeManager.EnumeratePrototypes<TechnologyPrototype>()
            .Where(p => p.Discipline == techDiscipline.ID && !p.Hidden).ToList();

        var percentage = (float)component.UnlockedTechnologies
            .Where(x => PrototypeManager.Index<TechnologyPrototype>(x).Discipline == techDiscipline.ID)
            .Count() / (float)allTech.Count * 100f;

        return (int)Math.Clamp(percentage, 0, 100);
    }
}
