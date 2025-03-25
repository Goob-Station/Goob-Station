using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Toggleable;

namespace Content.Goobstation.Shared.Toggle;

public sealed class ItemToggleColorSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemToggleColorComponent, ItemToggledEvent>(OnLightToggled);
    }

    private void OnLightToggled(Entity<ItemToggleColorComponent> ent, ref ItemToggledEvent args)
    {
        _appearance.SetData(ent, ToggleableLightVisuals.Enabled, args.Activated);
    }
}
