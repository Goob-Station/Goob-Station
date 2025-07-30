using Content.Shared.GameTicking.Components;
using Content.Server.Psionics.Glimmer;
using Content.Shared.Psionics.Glimmer;
using Content.Pirate.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Server.GameTicking;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.StationEvents.Events;

public sealed class GlimmerEventSystem : StationEventSystem<GlimmerEventComponent>
{
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Started(EntityUid uid, GlimmerEventComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Check if current glimmer level allows this event to run
        var currentGlimmer = _glimmerSystem.GlimmerOutput;

        if (currentGlimmer < component.MinimumGlimmer || currentGlimmer > component.MaximumGlimmer)
        {
            // Cancel the event if glimmer requirements are not met
            Log.Warning($"Glimmer event {ToPrettyString(uid)} cancelled due to glimmer requirements. " +
                       $"Current: {currentGlimmer}, Required: {component.MinimumGlimmer}-{component.MaximumGlimmer}");

            // End the game rule immediately
            _gameTicker.EndGameRule(uid);
            return;
        }
    }

    protected override void Ended(EntityUid uid, GlimmerEventComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        var glimmerBurned = RobustRandom.Next(component.GlimmerBurnLower, component.GlimmerBurnUpper);
        _glimmerSystem.DeltaGlimmerInput(-glimmerBurned);

        var reportEv = new GlimmerEventEndedEvent(component.SophicReport, glimmerBurned);
        RaiseLocalEvent(reportEv);
    }
}

public sealed class GlimmerEventEndedEvent : EntityEventArgs
{
    public string Message = "";
    public int GlimmerBurned = 0;

    public GlimmerEventEndedEvent(string message, int glimmerBurned)
    {
        Message = message;
        GlimmerBurned = glimmerBurned;
    }
}
