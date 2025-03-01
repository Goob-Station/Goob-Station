using System.Numerics;
using Content.Shared._Lavaland.GPS;
using Content.Shared.GameTicking;

namespace Content.Server._Lavaland.GPS;

public sealed class LavalandGpsSystem : SharedLavalandGpsSystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);
        SubscribeLocalEvent<GpsSignalComponent, MapInitEvent>(OnSignalInit);
        SubscribeLocalEvent<GpsSignalComponent, EntityTerminatingEvent>(OnSignalDelete);

        Subs.BuiEvents<SignalLocatorComponent>(SignalLocatorUiKey.Key,
            subs =>
        {
            subs.Event<GpsRefreshMessage>(OnRefresh);
            subs.Event<GpsRefreshRangeMessage>(OnRefreshRange);
            subs.Event<GpsRefreshModeMessage>(OnRefreshMode);
        });
    }

    // that is just better compared to EntityQuerying it every second
    private readonly List<LavalandSignal> _signals = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var locatorQuery = EntityQueryEnumerator<SignalLocatorComponent, TransformComponent>();

        while (locatorQuery.MoveNext(out var uid, out var locator, out var xform))
        {
            if (locator.RefreshType != GpsRefreshType.Auto)
                continue;

            locator.UpdateAccumulator += frameTime;

            if (locator.UpdateAccumulator < locator.UpdateFrequency)
                continue;

            locator.UpdateAccumulator = 0;

            UpdateLocator((uid, locator, xform));
        }
    }

    /// <summary>
    /// Updates locator according to all active signals and dirties it.
    /// </summary>
    public void UpdateLocator(Entity<SignalLocatorComponent?, TransformComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2))
            return;

        // Location name
        UpdateLocation(ent.Comp1, ent.Comp2);

        var pos = (Vector2i) _xform.GetWorldPosition(ent.Comp2);
        // Depending on what locator requests, search for signals in available range.
        var signals =
            _signals.FindAll(signalPos => Vector2.Distance(pos, signalPos.Position) > ent.Comp1.GetCurrentRange);

        if (ent.Comp1.RangeType != GpsRangeType.Max)
        {
            // Remove every signal that is not on our map
            signals.RemoveAll(x => x.MapId != ent.Comp2.MapID);
        }

        // Update component
        ent.Comp1.Signals = signals;
        Dirty(ent.Owner, ent.Comp1);

        // Update UI
        var state = new GpsSignalLocatorState(signals, (pos, ent.Comp2.MapID.ToString()));
        _ui.SetUiState(ent.Owner, SignalLocatorUiKey.Key, state);
    }

    private void UpdateLocation(SignalLocatorComponent locator, TransformComponent xform)
    {
        if (xform.GridUid != null)
        {
            locator.LocationName = Name(xform.GridUid.Value);
        }
        else if (xform.MapUid != null)
        {
            locator.LocationName = Name(xform.MapUid.Value);
        }
        else
        {
            locator.LocationName = Loc.GetString("signal-locator-location-unknown");
        }
    }

    private void OnRefresh(Entity<SignalLocatorComponent> ent, ref GpsRefreshMessage args)
    {
        UpdateLocator((ent.Owner, ent.Comp));
    }

    private void OnRefreshRange(Entity<SignalLocatorComponent> ent, ref GpsRefreshRangeMessage args)
    {
        ent.Comp.RangeType = args.RangeType;
        UpdateLocator((ent.Owner, ent.Comp));
    }

    private void OnRefreshMode(Entity<SignalLocatorComponent> ent, ref GpsRefreshModeMessage args)
    {
        ent.Comp.RefreshType = args.RefreshType;
        UpdateLocator((ent.Owner, ent.Comp));
    }

    private void OnSignalInit(Entity<GpsSignalComponent> ent, ref MapInitEvent args)
    {
        var xform = Transform(ent);
        var position = (Vector2i) _xform.GetWorldPosition(xform);
        _signals.Add(new LavalandSignal(GetNetEntity(ent), ent.Comp.SignalName, position, xform.MapID));
    }

    private void OnSignalDelete(Entity<GpsSignalComponent> ent, ref EntityTerminatingEvent args)
    {
        var signal = _signals.Find(x => x.Entity == GetNetEntity(ent));
        _signals.Remove(signal);
    }

    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        _signals.Clear();
    }
}
