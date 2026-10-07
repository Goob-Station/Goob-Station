using Content.Shared.Examine;
using Content.Shared.CCVar;
using Content.Shared.SSDIndicator;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.SSDTimer;

public sealed partial class SSDTimerSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameTiming _timing = default!;
	
	private float _icSsdSleepTime;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SSDIndicatorComponent, ExaminedEvent>(OnExamined);
		
        _cfg.OnValueChanged(CCVars.ICSSDSleepTime, obj => _icSsdSleepTime = obj, true);

    }
	
    private void OnExamined(EntityUid uid, SSDIndicatorComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !component.IsSSD)
            return;

		var timeFellAsleep = component.FallAsleepTime - TimeSpan.FromSeconds(_icSsdSleepTime);
		var time = _timing.CurTime - timeFellAsleep;
		args.PushMarkup(Loc.GetString("comp-ssd-person-examined", ("ent", uid), ("time", (int) time.TotalMinutes)));
    }
}
