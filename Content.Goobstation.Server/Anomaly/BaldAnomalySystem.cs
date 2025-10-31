using Content.Server.Anomaly;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Humanoid;
using Content.Server.Physics.Components;
using Content.Shared.Anomaly.Components;
using Content.Shared.Chat;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Anomaly;

/// <summary>
/// This handles the Bald anomalitys systems
/// </summary>
public sealed class BaldAnomalySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly AnomalySystem _anomaly = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BaldAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<BaldAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical);
        SubscribeLocalEvent<BaldAnomalyComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<BaldAnomalyComponent, AnomalyShutdownEvent>(OnShutdown);
        SubscribeLocalEvent<BaldAnomalyComponent, AnomalySeverityChangedEvent>(OnSeverityChanged);
    }

    /// <summary>
    ///  On Pulse evryone in range gets made bald
    /// evryone who is bald praises baldness
    /// </summary>
    /// <param name="anomaly"></param>
    /// <param name="args"></param>
    private void OnPulse(Entity<BaldAnomalyComponent> anomaly, ref AnomalyPulseEvent args)
    {
        if (args.Severity >= 1) // pulse won't happen just before a critical
            return;

        var range = anomaly.Comp.BaseRange * args.Severity * args.PowerModifier;
        var crew = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(anomaly).Coordinates, range);

        foreach (var (ent, comp) in crew)
        {
            if (MakeBald(anomaly, ent, comp))
                continue;
            if (_random.Next(100) < anomaly.Comp.SpeakChance)
                continue;

            var line = Loc.GetString("anomaly-bald-say-"+ _random.Next(anomaly.Comp.SayLines));
            _chatSystem.TrySendInGameICMessage(ent,line, InGameICChatType.Speak,true);
        }
    }

    /// <summary>
    /// on critical gibs one non-bald crew member
    /// if PowerModifier is 100%  or more
    /// </summary>
    /// <param name="anomaly"></param>
    /// <param name="args"></param>
    private void OnSupercritical(Entity<BaldAnomalyComponent> anomaly, ref AnomalySupercriticalEvent args)
    {
        // gib one random non-bald crew member
        if (args.PowerModifier >= 1)//does not trigger if it's a weak anomaly
        {
            var range = anomaly.Comp.BaseRange * 10;
            var crew = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(anomaly).Coordinates, range);
            var potentialTargets = new List<EntityUid>();
            foreach (var (ent, comp) in crew)
            {
               if ( _mobState.IsDead(ent))
                   continue;// dont count the dead
               if (!comp.MarkingSet.TryGetCategory(MarkingCategories.Hair, out var hair) || hair.Count <= 0)
                   continue; // is already bald  or cant have hair
               //TODO  probably have a check that someone that is fresh off the boat dont get gibed

               potentialTargets.Add(ent);
            }

            if (potentialTargets.Count > 0)
            {
                var victim = potentialTargets[_random.Next(potentialTargets.Count)];
                _chatSystem.TrySendInGameICMessage(victim,Loc.GetString("anomaly-bald-cri"), InGameICChatType.Speak,false);
                _bodySystem.GibBody(victim);
            }
        }
    }

    /// <summary>
    /// on interaction with an empty hand makes you bald
    /// </summary>
    /// <param name="anomaly"></param>
    /// <param name="args"></param>
    private void OnInteract(Entity<BaldAnomalyComponent> anomaly, ref InteractHandEvent args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(args.User,out var appearance))
            return;

        MakeBald(anomaly, args.User, appearance);
    }

    /// <summary>
    ///  helper function for making target bald
    /// </summary>
    /// <param name="anomaly"></param>
    /// <param name="target"></param> target to be made bald
    /// <param name="appearance"></param>
    /// <returns> returns true if made bald</returns>
    private bool MakeBald(Entity<BaldAnomalyComponent> anomaly ,EntityUid target, HumanoidAppearanceComponent appearance)
    {
        if (!appearance.MarkingSet.TryGetCategory(MarkingCategories.Hair, out var hair) || hair.Count <= 0)
            return false;

        // remove hair
        _humanoid.RemoveMarking(target, MarkingCategories.Hair, 0);
        _audio.PlayPredicted(anomaly.Comp.Sound, target, target);
        return true;
    }

    /// <summary>
    /// on anomaly being removed spawn a copy somewhere else on the station
    /// </summary>
    /// <param name="anomaly"></param>
    /// <param name="args"></param>
    private void OnShutdown(Entity<BaldAnomalyComponent> anomaly, ref AnomalyShutdownEvent args)
    {
        var proto = MetaData(anomaly.Owner).EntityPrototype;
        var grid = Transform(anomaly).GridUid;
        if (proto is null || grid is null)
            return;

        _anomaly.SpawnOnRandomGridLocation(grid.Value, proto.ID);
    }

    private void OnSeverityChanged(Entity<BaldAnomalyComponent> anomaly, ref AnomalySeverityChangedEvent args)
    {
        var severity = args.Severity*100;

        if (severity < 50)
        {
            RemCompDeferred<RandomWalkComponent>(anomaly.Owner);
            EnsureComp<PullableComponent>(anomaly.Owner);
            return;
        }
        if (80 < severity)
        {
            _pulling.StopAllPulls(anomaly.Owner);
            EnsureComp<RandomWalkComponent>(anomaly.Owner);
            return;
        }
        _pulling.StopAllPulls(anomaly.Owner);
        RemCompDeferred<PullableComponent>(anomaly.Owner);
        RemCompDeferred<RandomWalkComponent>(anomaly.Owner);
    }
}
