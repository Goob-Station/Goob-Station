using Content.Shared._CorvaxGoob.PowerToggle;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Client._CorvaxGoob.PowerToggle;

public sealed partial class TogglePowerSystem : SharedTogglePowerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TogglePowerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<TogglePowerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<TogglePowerComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (entity.Comp.IsTurnedOn)
            args.PushMarkup(Loc.GetString("power-toggle-status-on"));
        else
            args.PushMarkup(Loc.GetString("power-toggle-status-off"));
    }

    private void OnGetVerbs(Entity<TogglePowerComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        var target = args.Target;
        var user = args.User;

        var verb = new AlternativeVerb
        {
            Text = Loc.GetString("power-toggle-verb"),
            Act = () => VerbPowerToggle(entity, target, user),
            Priority = -100
        };
        args.Verbs.Add(verb);
    }

    private void VerbPowerToggle(Entity<TogglePowerComponent> entity, EntityUid target, EntityUid user)
    {
        if (_timing.CurTime < entity.Comp.NextToggle)
            return;

        entity.Comp.NextToggle = _timing.CurTime + entity.Comp.ToggleInterval;
        entity.Comp.IsTurnedOn = !entity.Comp.IsTurnedOn;

        RaiseNetworkEvent(new TogglePowerMessage(GetNetEntity(target), GetNetEntity(user)));
    }
}
