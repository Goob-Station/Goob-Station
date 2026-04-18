using Content.Goobstation.Shared.Terror;
using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Handles terror-specific web effects when a step trap is triggered.
/// Got torn apart in favor of making systems generic so now all it does is pop-ups.
/// </summary>
public sealed class TerrorWebSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorWebComponent, StepTrapTriggeredEvent>(OnTriggered);
    }

    private void OnTriggered(EntityUid uid, TerrorWebComponent _, ref StepTrapTriggeredEvent ev)
    {
        if (HasComp<InfestedWebComponent>(uid))
        {
            _popup.PopupPredicted(Loc.GetString("sticky-web-infested"), ev.Tripper, ev.Tripper, PopupType.MediumCaution);
            EnsureComp<InfestedComponent>(ev.Tripper);
            return;
        }

        if (HasComp<InjectorTileComponent>(uid))
        {
            _popup.PopupPredicted(Loc.GetString("sticky-web-injected"), ev.Tripper, ev.Tripper, PopupType.MediumCaution);
            return;
        }

        _popup.PopupPredicted(Loc.GetString("sticky-web-generic"), ev.Tripper, ev.Tripper, PopupType.MediumCaution);
    }
}
