// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.NameModifier.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Goobstation.Shared.Nutrition.EntitySystems;

public class FoodSequenceSpriteSystem : SharedFoodSequenceSystem
{
    private EntityQuery<NameModifierComponent> _modifierQuery;

    public override void Initialize()
    {
        base.Initialize();

        _modifierQuery = GetEntityQuery<NameModifierComponent>();

        SubscribeLocalEvent<FoodSequenceElementComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<FoodSequenceElementComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.Entries.Count != 0)
            return;

        var defaultEntry = new FoodSequenceElementEntry();

        var meta = MetaData(ent);
        var name = _modifierQuery.CompOrNull(ent)?.BaseName ?? meta.EntityName;
        defaultEntry.Name = name.Replace(" ", string.Empty);
        defaultEntry.Proto = meta.EntityPrototype?.ID;

        ent.Comp.Entries.Add("default", defaultEntry);
    }
}
