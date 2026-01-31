using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Toggleable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared.Timing;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Goobstation.Server.Religion.OnPray.TimedToggleOnPray;

public sealed class TimedToggleOnPraySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;

    private EntityQuery<TimedToggleOnPrayComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<TimedToggleOnPrayComponent>();
        SubscribeLocalEvent<TimedToggleOnPrayComponent, AlternatePrayEvent>(OnPray);
        SubscribeLocalEvent<TimedToggleOnPrayComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TimedToggleOnPrayComponent, MapInitEvent>(OnMapInit);
    }

    private void OnStartup(Entity<TimedToggleOnPrayComponent> ent, ref ComponentStartup args)
    {
        UpdateVisuals(ent);
    }

    private void OnMapInit(Entity<TimedToggleOnPrayComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.Activated)
            return;

        var ev = new ItemToggledEvent(Predicted: ent.Comp.Predictable, Activated: ent.Comp.Activated, User: null);
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnPray(Entity<TimedToggleOnPrayComponent> ent, ref AlternatePrayEvent args)
    {
        if (_delay.IsDelayed(ent.Owner))
            return;

        TryActivate((ent, ent.Comp), args.User, predicted: ent.Comp.Predictable);
    }

    public bool TryActivate(Entity<TimedToggleOnPrayComponent?> ent, EntityUid? user = null, bool predicted = true)
    {
        if (!_query.Resolve(ent, ref ent.Comp, false))
            return false;

        var uid = ent.Owner;
        var comp = ent.Comp;
        if (comp.Activated)
            return true;

        var attempt = new ItemToggleActivateAttemptEvent(user);
        RaiseLocalEvent(uid, ref attempt);

        if (!comp.Predictable)
            predicted = false;

        Activate((uid, comp), predicted, user);
        return true;
    }

    private void Activate(Entity<TimedToggleOnPrayComponent> ent, bool predicted, EntityUid? user = null)
    {
        var (uid, comp) = ent;
        var soundToPlay = comp.SoundActivate;

        if (!_delay.IsDelayed(ent.Owner))
            _delay.TryResetDelay(ent.Owner);

        _audio.PlayPredicted(soundToPlay, uid, user);

        comp.Activated = true;
        comp.Time = _timing.CurTime + TimeSpan.FromSeconds(comp.Duration);
        comp.TimerRun = true;
        UpdateVisuals((uid, comp));

        var toggleUsed = new ItemToggledEvent(predicted, Activated: true, user);
        RaiseLocalEvent(uid, ref toggleUsed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TimedToggleOnPrayComponent>();
        while (query.MoveNext(out var ent, out var time))
        {
            if (time.TimerRun == false)
                continue;

            if (_timing.CurTime >= time.Time == false)
                continue;

            time.TimerRun = false;
            Deactivate((ent, time), time.Predictable);
        }
    }

    private void Deactivate(Entity<TimedToggleOnPrayComponent> ent, bool predicted, EntityUid? user = null)
    {
        var (uid, comp) = ent;
        var soundToPlay = comp.SoundDeactivate;

        _audio.PlayPredicted(soundToPlay, uid, user);

        comp.Activated = false;
        UpdateVisuals((uid, comp));

        var toggleUsed = new ItemToggledEvent(predicted, Activated: false, user);
        RaiseLocalEvent(uid, ref toggleUsed);
    }

    private void UpdateVisuals(Entity<TimedToggleOnPrayComponent> ent)
    {
        if (TryComp(ent, out AppearanceComponent? appearance))
        {
            _appearance.SetData(ent, ToggleableVisuals.Enabled, ent.Comp.Activated, appearance);
        }
    }
}
