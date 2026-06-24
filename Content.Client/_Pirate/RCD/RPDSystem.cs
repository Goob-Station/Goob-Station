using Content.Client.Items;
using Content.Client.Message;
using Content.Shared.RCD.Components;
using Content.Shared.RCD.Systems;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._Pirate.RCD;

public sealed class RPDSystem : EntitySystem
{
    private RCDSystem _rcdSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        _rcdSystem = EntityManager.System<RCDSystem>();
        Subs.ItemStatus<RCDComponent>(OnItemStatus);
    }

    private Control? OnItemStatus(Entity<RCDComponent> entity)
    {
        return entity.Comp.IsRpd || entity.Comp.IsRPLD
            ? new RPDModeStatusControl(entity, _rcdSystem)
            : null;
    }

    private sealed class RPDModeStatusControl : Control
    {
        private readonly RichTextLabel _label = new()
        {
            StyleClasses = { "ItemStatus" },
        };

        private readonly EntityUid _uid;
        private readonly RCDSystem _rcdSystem;

        public RPDModeStatusControl(Entity<RCDComponent> entity, RCDSystem rcdSystem)
        {
            _uid = entity.Owner;
            _rcdSystem = rcdSystem;
            AddChild(_label);
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            var currentMode = _rcdSystem.GetCurrentRpdMode(_uid);
            var modeKey = $"rcd-rpd-mode-{currentMode.ToString().ToLowerInvariant()}";
            var modeName = Robust.Shared.Localization.Loc.GetString(modeKey);

            _label.SetMarkup(Robust.Shared.Localization.Loc.GetString("rcd-item-status-mode",
                ("mode", $"[color=cyan]{modeName}[/color]")));
        }
    }
}
