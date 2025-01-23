using Content.Shared._Goobstation.Wizard;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;

namespace Content.Client._Goobstation.Wizard.Systems;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ActionTargetMarkSystem _mark = default!;

    public event Action? StopTargeting;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardComponent, GetStatusIconsEvent>(GetWizardIcon);

        SubscribeNetworkEvent<StopTargetingEvent>(OnStopTargeting);
    }

    public void SetSwapSecondaryTarget(EntityUid user, EntityUid? target, EntityUid action)
    {
        if (target == null || user == target)
        {
            _mark.SetMark(null);
            RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), null));
            return;
        }

        _mark.SetMark(target);
        RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), GetNetEntity(target.Value)));
    }

    private void OnStopTargeting(StopTargetingEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession != _player.LocalSession)
            return;

        StopTargeting?.Invoke();
    }

    private void GetWizardIcon(Entity<WizardComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ProtoMan.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
