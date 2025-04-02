using Content.Server.DeviceLinking.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Events;
using Content.Goobstation.Shared.Contraband;
using Content.Server.Power.EntitySystems;
using Robust.Server.Audio;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Contraband;

public sealed class ContrabandDetectorSystem : SharedContrabandDetectorSystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContrabandDetectorComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(Entity<ContrabandDetectorComponent> detector, ref StartCollideEvent args)
    {
        var (uid, component) = detector;

        if (!_powerReceiverSystem.IsPowered(detector.Owner))
            return;

        if (component.Scanned.ContainsKey(args.OtherEntity))
            return;

        component.Scanned.Add(args.OtherEntity, _timing.CurTime + component.ScanTimeOut);

        // We don't need to scan if IsFalseScanning.
        bool isDetected = false;
        if (!component.IsFalseScanning)
        {
            var list = FindContraband(args.OtherEntity);

            // XOR method to check both false negative and false positive outcomes
            isDetected = list.Count > 0 ^ _random.Prob(component.FalseDetectingChance);
        }

        if (isDetected)
        {
            _audioSystem.PlayPvs(component.Detect, uid);
            _deviceLink.SendSignal(uid, "SignalContrabandDetected", true);
            component.State = ContrabandDetectorState.Alarm;
        }
        else
        {
            _audioSystem.PlayPvs(component.NoDetect, uid);
            _deviceLink.SendSignal(uid, "SignalContrabandDetected", false);
            component.State = ContrabandDetectorState.Scan;
        }

        component.LastScanTime = _timing.CurTime;
        UpdateVisuals(detector);
        Dirty(detector);
    }
}
