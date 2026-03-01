using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using System.Text;

namespace Content.Goobstation.Shared.Clothing.Systems;

[Virtual]
public partial class SharedClothingCoatingSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingCoatingComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CoatedClothingComponent, ExaminedEvent>(OnExamined);
    }

    private void OnAfterInteract(Entity<ClothingCoatingComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.Target.HasValue
        || !TryComp<ClothingComponent>(args.Target, out var clothing))
            return;

        var target = args.Target.Value;
        EntityManager.AddComponents(target, ent.Comp.Components, false);

        var coated = EnsureComp<CoatedClothingComponent>(target);
        coated.CoatingNames.Add(ent.Comp.CoatingName);
        _popup.PopupEntity(Loc.GetString("clothing-coating-success", ("target", target), ("source", ent)), target);
        Dirty(target, coated);

        QueueDel(ent);
        args.Handled = true;
    }

    private void OnExamined(Entity<CoatedClothingComponent> ent, ref ExaminedEvent args)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < ent.Comp.CoatingNames.Count; i++)
        {
            sb.Append(Loc.GetString(ent.Comp.CoatingNames[i]));
            if (i >= ent.Comp.CoatingNames.Count - 1) sb.Append(", ");
        }
        args.PushMarkup(Loc.GetString("clothing-coating-inspect", ("coatings", sb.ToString())));
    }
}
