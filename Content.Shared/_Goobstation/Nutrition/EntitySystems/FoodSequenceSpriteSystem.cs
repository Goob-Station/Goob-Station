using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;


namespace Content.Shared._Goobstation.Nutrition.EntitySystems
{
    public class FoodSequenceSpriteSystem : SharedFoodSequenceSystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<FoodSequenceElementComponent, ComponentStartup>(OnComponentStartup);
        }

        private void OnComponentStartup(Entity<FoodSequenceElementComponent> ent, ref ComponentStartup args)
        {
            if (ent.Comp.Entries.Count == 0)
            {
                var defaultEntry = new FoodSequenceElementEntry();

                if (TryComp<MetaDataComponent>(ent, out var meta))
                {
                    defaultEntry.Name = meta.EntityName.Replace(" ", string.Empty);
                    defaultEntry.Proto = meta.EntityPrototype?.ID;
                }

                ent.Comp.Entries.Add("default", defaultEntry);
            }
        }

    }
}
