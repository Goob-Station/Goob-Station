using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Pops up a message when a terror web is stepped on, and infests the tripper if configured to.
/// </summary>
public sealed class TerrorWebSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorWebComponent, StepTrapTriggeredEvent>(OnTriggered);
    }

    private void OnTriggered(EntityUid uid, TerrorWebComponent comp, ref StepTrapTriggeredEvent ev)
    {
        _popup.PopupPredicted(Loc.GetString("sticky-web-generic"), ev.Tripper, ev.Tripper, PopupType.MediumCaution);

        if (comp.InflictsInfested)
        {
            _status.TryAddStatusEffect(ev.Tripper, "Infested", out _, TimeSpan.FromMinutes(30));
        }
    }
}
