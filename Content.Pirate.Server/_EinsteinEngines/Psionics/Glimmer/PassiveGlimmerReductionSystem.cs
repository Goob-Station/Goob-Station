using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Content.Pirate.Common.CCVar;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.Psionics.Glimmer;

/// <summary>
/// Handles the passive reduction of glimmer.
/// </summary>
public sealed class PassiveGlimmerReductionSystem : EntitySystem
{
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    /// List of glimmer values spaced by minute.
    public List<int> GlimmerValues = new();
    public TimeSpan NextUpdateTime = default!;
    public TimeSpan LastUpdateTime = default!;

    private TimeSpan _targetUpdatePeriod;
    private float _glimmerLinearDecay;
    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();
        _enabled = _cfg.GetCVar(CCVars.GlimmerEnabled);
        _cfg.OnValueChanged(CCVars.GlimmerLinearDecayPerSecond, UpdatePassiveGlimmer, true);
        _cfg.OnValueChanged(CCVars.GlimmerEnabled, value => _enabled = value, true);
        _cfg.OnValueChanged(CCVars.GlimmerDecayUpdateInterval, value => _targetUpdatePeriod = TimeSpan.FromSeconds(value), true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_enabled)
            return;

        var glimmerDecay = _glimmerLinearDecay * frameTime;

        _glimmerSystem.DeltaGlimmerOutput(-glimmerDecay);
        GlimmerValues.Add((int) Math.Round(_glimmerSystem.GlimmerOutput));
    }

    private void UpdatePassiveGlimmer(float value) => _glimmerLinearDecay = value;
}
