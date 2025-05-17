using Content.Shared.Item.ItemToggle;
using Content.Shared.Popups;
using Content.Shared.Tools.Components;

namespace Content.Goobstation.Shared.Tools;

public sealed class ItemToggleToolSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemToggleToolComponent, ToolUseAttemptEvent>(OnToolUseAttempt);
    }

    private void OnToolUseAttempt(Entity<ItemToggleToolComponent> ent, ref ToolUseAttemptEvent args)
    {
        if (_toggle.IsActivated(ent.Owner))
            return;

        _popup.PopupClient(Loc.GetString("item-toggle-tool-turn-on", ("tool", ent)), ent, args.User);
        args.Cancel();
    }
}
