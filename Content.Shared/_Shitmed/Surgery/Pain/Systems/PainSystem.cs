using Content.Shared._Shitmed.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Surgery.Pain.Components;
using Content.Shared._Shitmed.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Random;
namespace Content.Shared._Shitmed.Surgery.Pain.Systems;

[Virtual]
public sealed partial class PainSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    [Dependency] private readonly SharedAudioSystem _IHaveNoMouthAndIMustScream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    [Dependency] private readonly MobStateSystem _mobState = default!;

    [Dependency] private readonly StandingStateSystem _standing = default!;

    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NerveComponent, ComponentHandleState>(OnComponentHandleState);
        SubscribeLocalEvent<NerveComponent, ComponentGetState>(OnComponentGet);

        SubscribeLocalEvent<NerveComponent, BodyPartAddedEvent>(OnBodyPartAdded);
        SubscribeLocalEvent<NerveComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);

        SubscribeLocalEvent<NerveSystemComponent, MobStateChangedEvent>(OnMobStateChanged);

        InitAffliction();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _painJobQueue.Process();

        if (!_timing.IsFirstTimePredicted)
            return;

        using var query = EntityQueryEnumerator<NerveSystemComponent>();
        while (query.MoveNext(out var ent, out var nerveSystem))
        {
            // Whoops, organ damage!
            if (!TerminatingOrDeleted(ent))
                _painJobQueue.EnqueueJob(new PainTimerJob(this, (ent, nerveSystem), PainJobTime));
        }
    }

    private void OnComponentHandleState(EntityUid uid, NerveComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not NerveComponentState state)
            return;

        component.ParentedNerveSystem = GetEntity(state.ParentedNerveSystem);
        component.PainMultiplier = state.PainMultiplier;

        component.PainFeelingModifiers.Clear();
        foreach (var ((modEntity, id), modifier) in state.PainFeelingModifiers)
        {
            component.PainFeelingModifiers.Add((GetEntity(modEntity), id), modifier);
        }
    }

    private void OnComponentGet(EntityUid uid, NerveComponent comp, ref ComponentGetState args)
    {
        var state = new NerveComponentState();

        if (!TerminatingOrDeleted(comp.ParentedNerveSystem))
            state.ParentedNerveSystem = GetNetEntity(comp.ParentedNerveSystem);
        state.PainMultiplier = comp.PainMultiplier;

        foreach (var ((modEntity, id), modifier) in comp.PainFeelingModifiers)
        {
            if (!TerminatingOrDeleted(modEntity))
                state.PainFeelingModifiers.Add((GetNetEntity(modEntity), id), modifier);
        }

        args.State = state;
    }

    private void OnBodyPartAdded(EntityUid uid, NerveComponent nerve, ref BodyPartAddedEvent args)
    {
        Logger.Debug($"ONBODYPARTADDED 1/3: Adding nerve system nerves for {uid}, and body {args.Part.Comp.Body}");
        var bodyPart = Comp<BodyPartComponent>(uid);
        if (!bodyPart.Body.HasValue)
            return;

        Logger.Debug($"ONBODYPARTADDED 2/3: Trying to get nerve system for {bodyPart.Body.Value}");
        if (!_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var brainUid) || TerminatingOrDeleted(brainUid.Value))
        {
            Logger.Debug($"ONBODYPARTADDED 2/3: No nerve system found for {bodyPart.Body.Value}");
            return;
        }

        TryRemovePainMultiplier(brainUid.Value, MetaData(args.Part.Owner).EntityPrototype!.ID + "Loss");
        Logger.Debug($"ONBODYPARTADDED 3/3: Adding nerve system nerves for {brainUid.Value}, and part {args.Part.Owner}");
        UpdateNerveSystemNerves(brainUid.Value, bodyPart.Body.Value, Comp<NerveSystemComponent>(brainUid.Value));
    }

    private void OnBodyPartRemoved(EntityUid uid, NerveComponent nerve, ref BodyPartRemovedEvent args)
    {
        var bodyPart = Comp<BodyPartComponent>(uid);
        if (!bodyPart.Body.HasValue)
            return;

        if (!_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var brainUid) || TerminatingOrDeleted(brainUid.Value))
            return;

        UpdateNerveSystemNerves(brainUid.Value, bodyPart.Body.Value, Comp<NerveSystemComponent>(brainUid.Value));
    }

    private void OnMobStateChanged(EntityUid uid, NerveSystemComponent nerveSys, MobStateChangedEvent args)
    {
        switch (args.NewMobState)
        {
            case MobState.Critical:
                var sex = Sex.Unsexed;
                if (TryComp<HumanoidAppearanceComponent>(args.Target, out var humanoid))
                    sex = humanoid.Sex;

                CleanupSounds(nerveSys);
                PlayPainSound(args.Target, nerveSys, nerveSys.CritWhimpers[sex], AudioParams.Default.WithVolume(-12f));
                nerveSys.NextCritScream = _timing.CurTime + _random.Next(nerveSys.CritScreamsIntervalMin, nerveSys.CritScreamsIntervalMax);
                break;
            case MobState.Dead:
                CleanupSounds(nerveSys);
                break;
        }
    }

    private void UpdateNerveSystemNerves(EntityUid uid, EntityUid body, NerveSystemComponent component)
    {
        Logger.Debug($"UPDATENERVESYSTEMNERVES 1/3: Updating nerve system nerves for {uid}, and body {body}");
        component.Nerves.Clear();
        foreach (var bodyPart in _body.GetBodyChildren(body))
        {
            if (!TryComp<NerveComponent>(bodyPart.Id, out var nerve))
            {
                Logger.Debug($"UPDATENERVESYSTEMNERVES 2/3: No nerve component found for {bodyPart.Id}");
                continue;
            }

            component.Nerves.Add(bodyPart.Id, nerve);
            Dirty(uid, component);

            nerve.ParentedNerveSystem = uid;
            Logger.Debug($"UPDATENERVESYSTEMNERVES 3/3: Nerve component found & added for {bodyPart.Id}, and nerve system {uid}");
            Dirty(bodyPart.Id, nerve); // ヾ(≧▽≦*)o
        }
    }
}
