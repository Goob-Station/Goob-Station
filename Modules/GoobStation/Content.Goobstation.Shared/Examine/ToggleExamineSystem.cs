using Content.Shared.Examine;
using Content.Shared.Item.ItemToggle;

namespace Content.Goobstation.Shared.Examine;

public sealed class ToggleExamineSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleExamineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<ToggleExamineComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var enabled = _toggle.IsActivated(ent.Owner);
        args.PushMarkup(Loc.GetString(enabled ? ent.Comp.Enabled : ent.Comp.Disabled));
    }

}
