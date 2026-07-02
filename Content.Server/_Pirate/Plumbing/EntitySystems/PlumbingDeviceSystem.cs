using Content.Server._Pirate.Plumbing.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using SharedAppearanceSystem = Robust.Shared.GameObjects.SharedAppearanceSystem;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Manages plumbing devices and raises update events on them.
///     Individual device logic is handled by subscribing to <see cref="PlumbingDeviceUpdateEvent"/>.
///     Also handles plunger interactions for draining plumbing machines.
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingDeviceSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private PuddleSystem _puddleSystem = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    private static readonly ProtoId<TagPrototype> _plungerTag = "Plunger";

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<PlungeableComponent, InteractUsingEvent>(OnPlungeableInteractUsing);
        SubscribeLocalEvent<PlumbingDeviceComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    /// <summary>
    ///     When a plumbing device is unanchored, stop its running animation.
    /// </summary>
    private void OnAnchorChanged(Entity<PlumbingDeviceComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored && ent.Comp.RequireAnchored)
            _appearance.SetData(ent.Owner, PlumbingVisuals.Running, false);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var devicesToUpdate = new List<(EntityUid Uid, PlumbingDeviceComponent Device)>();

        var query = EntityQueryEnumerator<PlumbingDeviceComponent>();
        while (query.MoveNext(out var uid, out var device))
        {
            // Cached query for better performance
            if (device.RequireAnchored && (!_xformQuery.TryGetComponent(uid, out var xform) || !xform.Anchored))
                continue;

            if (curTime < device.NextUpdateTime)
                continue;

            devicesToUpdate.Add((uid, device));
        }

        // Shuffle to ensure fair distribution when multiple devices pull from same network
        _random.Shuffle(devicesToUpdate);

        foreach (var (uid, device) in devicesToUpdate)
        {
            device.NextUpdateTime = curTime + device.UpdateInterval;

            var ev = new PlumbingDeviceUpdateEvent((float)device.UpdateInterval.TotalSeconds);
            RaiseLocalEvent(uid, ref ev);
        }
    }

    /// <summary>
    ///     Handles plunger interactions to drain all solutions from a plungeable machine.
    /// </summary>
    private void OnPlungeableInteractUsing(Entity<PlungeableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_tag.HasTag(args.Used, _plungerTag))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(ent.Owner, out var solutionManager))
            return;

        var totalDrained = FixedPoint2.Zero;
        var coords = Transform(ent.Owner).Coordinates;

        foreach (var (name, soln) in _solutionSystem.EnumerateSolutions((ent.Owner, solutionManager)))
        {
            var solution = soln.Comp.Solution;

            if (solution.Volume <= 0)
                continue;

            // Drain all contents and spill onto floor
            var drained = _solutionSystem.SplitSolution(soln, solution.Volume);
            totalDrained += drained.Volume;

            _puddleSystem.TrySpillAt(coords, drained, out _);
        }

        if (totalDrained > 0)
        {
            _popup.PopupEntity(Loc.GetString("plumbing-drain-success", ("amount", totalDrained)), ent.Owner, args.User);
            _audio.PlayPvs(ent.Comp.DrainSound, ent.Owner);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("plumbing-drain-empty"), ent.Owner, args.User);
        }

        args.Handled = true;
    }
}
