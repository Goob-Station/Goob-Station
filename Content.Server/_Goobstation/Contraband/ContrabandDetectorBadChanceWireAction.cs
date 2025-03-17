using Content.Server.Wires;
using Content.Shared._Goobstation.Contraband;
using Content.Shared.Wires;

namespace Content.Server._Goobstation.Contraband;

[DataDefinition]
public sealed partial class ContrabandDetectorBadChanceWireAction : BaseToggleWireAction
{
    private ContrabandDetectorSystem _contrabandDetectorSystem = default!;

    public override Color Color { get; set; } = Color.DarkOrange;
    public override string Name { get; set; } = "wire-name-contraband-detector-chance";
    public override object? StatusKey { get; } = ContrabandDetectorChanceWireKey.StatusKey;
    public override object? TimeoutKey { get; } = ContrabandDetectorChanceWireKey.TimeoutKey;

    public override void Initialize()
    {
        base.Initialize();

        _contrabandDetectorSystem = EntityManager.System<ContrabandDetectorSystem>();
    }

    public override StatusLightState? GetLightState(Wire wire)
    {
        if (EntityManager.TryGetComponent<ContrabandDetectorComponent>(wire.Owner, out var component))
        {
            return component.IsFalseDetectingChanged
                ? StatusLightState.BlinkingSlow
                : StatusLightState.On;
        }

        return StatusLightState.Off;
    }

    public override void ToggleValue(EntityUid owner, bool setting)
    {
        if (EntityManager.TryGetComponent<ContrabandDetectorComponent>(owner, out var component))
        {
            _contrabandDetectorSystem.ChangeFalseDetectionChance((owner, component), component.FalseDetectingChanceMultiplier);
        }
    }

    public override bool GetValue(EntityUid owner)
    {
        return EntityManager.TryGetComponent<ContrabandDetectorComponent>(owner, out var component) && !component.IsFalseDetectingChanged;
    }
}
