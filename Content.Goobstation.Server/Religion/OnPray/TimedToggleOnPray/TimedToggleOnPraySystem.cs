using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Goobstation.Server.Religion.OnPray.TimedToggleOnPray;

public sealed partial class TimedToggleOnPraySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private EntityQuery<TimedToggleOnPrayComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<TimedToggleOnPrayComponent>();


        SubscribeLocalEvent<TimedToggleOnPrayComponent, AlternatePrayEvent>(OnPray);
        SubscribeLocalEvent<TimedToggleOnPrayComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TimedToggleOnPrayComponent, MapInitEvent>(OnMapInit);
        
        SubscribeLocalEvent<ItemToggleActiveSoundComponent, ItemToggledEvent>(UpdateActiveSound);
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
        var (uid, comp) = ent;
        var active = comp.Activated;

        if (active)
            return;

        Activate((uid, comp));
    }

    private void Activate(Entity<TimedToggleOnPrayComponent> ent, EntityUid? user = null)
    {
        var (uid, comp) = ent;
        var soundToPlay = comp.SoundActivate;
        var duration = comp.Duration;
        var predicted = true;

        _audio.PlayPredicted(soundToPlay, uid, user);

        comp.Activated = true;
        comp.Time = _timing.CurTime + TimeSpan.FromSeconds(comp.Duration);
        comp.TimerRun = true;
        UpdateVisuals((uid, comp));
        Dirty(uid, comp);

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

            if (_timing.CurTime > time.Time == false)
                continue;

            time.TimerRun = false;
            Deactivate((ent, time));
        }
    }

    private void Deactivate(Entity<TimedToggleOnPrayComponent> ent, EntityUid? user = null)
    {
        var (uid, comp) = ent;
        var soundToPlay = comp.SoundDeactivate;
        var predicted = true;
        _audio.PlayPredicted(soundToPlay, uid, user);

        comp.Activated = false;
        UpdateVisuals((uid, comp));
        Dirty(uid, comp);

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

    private void UpdateActiveSound(Entity<ItemToggleActiveSoundComponent> ent, ref ItemToggledEvent args)
    {
        var (uid, comp) = ent;
        if (!args.Activated)
        {
            comp.PlayingStream = _audio.Stop(comp.PlayingStream);
            return;
        }

        if (comp.ActiveSound != null && comp.PlayingStream == null)
        {
            var loop = comp.ActiveSound.Params.WithLoop(true);
            var stream = args.Predicted
                ? _audio.PlayPredicted(comp.ActiveSound, uid, args.User, loop)
                : _audio.PlayPvs(comp.ActiveSound, uid, loop);
            if (stream?.Entity is {} entity)
                comp.PlayingStream = entity;
        }
    }

}
