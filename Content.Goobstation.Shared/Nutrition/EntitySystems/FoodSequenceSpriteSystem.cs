// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.NameModifier.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Nutrition.EntitySystems;

public class FoodSequenceSpriteSystem : SharedFoodSequenceSystem
{
    // Yeah nah i agree fuck this.
    /*
    private EntityQuery<NameModifierComponent> _modifierQuery;

    public override void Initialize()
    {
        base.Initialize();

        _modifierQuery = GetEntityQuery<NameModifierComponent>();

        SubscribeLocalEvent<FoodSequenceElementComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<FoodSequenceElementComponent> ent, ref ComponentStartup args)
    {
        var _protoMan = IoCManager.Resolve<IPrototypeManager>();
        if (ent.Comp.Entries.Count != 0)
            return;

        var defaultEntry = new FoodSequenceElementEntry();

        var meta = MetaData(ent);
        var name = _modifierQuery.CompOrNull(ent)?.BaseName ?? meta.EntityName;
        defaultEntry.Name = name.Replace(" ", string.Empty);
        defaultEntry.Proto = meta.EntityPrototype?.ID;

        ent.Comp.Entries.Add("default", defaultEntry);
    }
    */
}
