using Content.Shared.Clothing;
using Content.Shared._Starlight.VentCrawling.Components;
using Content.Shared._Starlight.VentCrawling;

namespace Content.Server._Starlight.VentCrawling;

public sealed class VentCrawlerClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VentCrawlerClothingComponent, ClothingGotEquippedEvent>(OnClothingEquip);
        SubscribeLocalEvent<VentCrawlerClothingComponent, ClothingGotUnequippedEvent>(OnClothingUnequip);
    }

    private void OnClothingEquip(Entity<VentCrawlerClothingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        var comp = AddComp<VentCrawlerComponent>(args.Wearer);
        comp.AllowInventory = false;
    }

    private void OnClothingUnequip(Entity<VentCrawlerClothingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        RemComp<VentCrawlerComponent>(args.Wearer);
    }
}
