using System.Linq;
using Content.Shared.Contests;
using Content.Shared.DoAfter;
using Content.Shared.Ghost;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Content.Shared.InteractionVerbs.InteractionPopupPrototype.Prefix;
using static Content.Shared.InteractionVerbs.InteractionVerbPrototype.ContestType;
using static Content.Shared.InteractionVerbs.InteractionVerbPrototype.EffectTargetSpecifier;

namespace Content.Shared.InteractionVerbs;

public abstract class SharedInteractionVerbsSystem : EntitySystem
{
    private readonly InteractionAction.VerbDependencies _verbDependencies = new();
    private List<InteractionVerbPrototype> _globalPrototypes = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfters = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        IoCManager.InjectDependencies(_verbDependencies);

        LoadGlobalVerbs();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        SubscribeLocalEvent<InteractionVerbsComponent, GetVerbsEvent<InteractionVerb>>(OnGetOthersVerbs);
        SubscribeLocalEvent<OwnInteractionVerbsComponent, GetVerbsEvent<InnateVerb>>(OnGetOwnVerbs);
        SubscribeLocalEvent<InteractionVerbDoAfterEvent>(OnDoAfterFinished);
    }

    private void LoadGlobalVerbs()
    {
        _globalPrototypes = _protoMan.EnumeratePrototypes<InteractionVerbPrototype>()
            .Where(v => v is { Global: true, Abstract: false })
            .ToList();
    }

    #region event handling

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<InteractionVerbPrototype>())
            return;

        LoadGlobalVerbs();
    }

    private void OnGetOthersVerbs(Entity<InteractionVerbsComponent> entity, ref GetVerbsEvent<InteractionVerb> args)
    {
        // Global verbs are not added here since OnGetOwnVerbs already adds them
        AddAll(entity.Comp.AllowedVerbs.Select(_protoMan.Index), args, () => new InteractionVerb());
    }

    private void OnGetOwnVerbs(Entity<OwnInteractionVerbsComponent> entity, ref GetVerbsEvent<InnateVerb> args)
    {
        var allVerbs = entity.Comp.AllowedVerbs;

        var getVerbsEv = new GetInteractionVerbsEvent(allVerbs);
        RaiseLocalEvent(entity, ref getVerbsEv);

        // Global verbs are added here because they should be allowed even on entities that do not define any interactions
        AddAll(allVerbs.Select(_protoMan.Index).Union(_globalPrototypes), args, () => new InnateVerb());
    }

    private void OnDoAfterFinished(InteractionVerbDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Handled || !_protoMan.TryIndex(ev.VerbPrototype, out var proto))
            return;

        PerformVerb(proto, ev.VerbArgs!);
        ev.Handled = true;
    }

    #endregion

    #region public api

    /// <summary>
    ///     Starts the verb, checking if it can be performed first, unless forced.
    ///     Upon success, this method will either start a do-after, or pass control to <see cref="PerformVerb"/>.
    /// </summary>
    // TODO this function is an active battlefield
    public bool StartVerb(InteractionVerbPrototype proto, InteractionArgs args, bool force = false)
    {
        if (!TryComp<OwnInteractionVerbsComponent>(args.User, out var ownInteractions)
            || !force && CheckVerbCooldown(proto, args, out _, ownInteractions))
            return false;

        // If contest advantage wasn't calculated yet, calculate it now and ensure it's in the allowed range
        var contestAdvantageValid = true;
        if (args.ContestAdvantage is null)
            CalculateAdvantage(proto, ref args, out contestAdvantageValid);

        if (!_net.IsClient
            && !force
            && (!contestAdvantageValid || proto.Action?.CanPerform(args, proto, true, _verbDependencies) != true))
        {
            CreateVerbEffects(proto.EffectFailure, Fail, proto, args);
            return false;
        }

        var attemptEv = new InteractionVerbAttemptEvent(proto, args);
        RaiseLocalEvent(args.User, ref attemptEv);
        RaiseLocalEvent(args.Target, ref attemptEv);

        if (attemptEv.Cancelled)
        {
            CreateVerbEffects(proto.EffectFailure, Fail, proto, args);
            return false;
        }
        if (attemptEv.Handled)
            return true;

        var cooldown = proto.Cooldown;
        var delay = proto.Delay;
        if (proto.ContestDelay)
            delay /= args.ContestAdvantage!.Value;
        if (proto.ContestCooldown)
            cooldown /= args.ContestAdvantage!.Value;

        StartVerbCooldown(proto, args, cooldown, ownInteractions);

        // Delay can become zero if the contest advantage is infinity or just really large...
        if (delay <= TimeSpan.Zero)
        {
            PerformVerb(proto, args);
            return true;
        }

        var doAfter = new DoAfterArgs(proto.DoAfter)
        {
            User = args.User,
            Target = args.Target,
            EventTarget = EntityUid.Invalid, // Raised broadcast
            Broadcast = true,
            BreakOnHandChange = proto.RequiresHands,
            NeedHand = proto.RequiresHands,
            RequireCanInteract = proto.RequiresCanInteract,
            Delay = delay,
            Event = new InteractionVerbDoAfterEvent(proto.ID, args)
        };

        var isSuccess = _doAfters.TryStartDoAfter(doAfter);
        if (isSuccess)
            CreateVerbEffects(proto.EffectDelayed, Delayed, proto, args);

        return isSuccess;
    }

    /// <summary>
    ///     Performs an additional CanPerform check (unless forced) and then actually performs the action of the verb
    ///     and shows a success/failure popup.
    /// </summary>
    /// <remarks>This does nothing on client, as the client has no clue about verb actions. Only the server should ever perform verbs.</remarks>
    public void PerformVerb(InteractionVerbPrototype proto, InteractionArgs args, bool force = false)
    {
        if (_net.IsClient)
            return; // this leads to issues

        if (!proto.Action!.CanPerform(args, proto, false, _verbDependencies) && !force
            || !proto.Action.Perform(args, proto, _verbDependencies))
        {
            CreateVerbEffects(proto.EffectFailure, Fail, proto, args);
            return;
        }

        CreateVerbEffects(proto.EffectSuccess, Success, proto, args);
    }

    #endregion

    #region private api

    /// <summary>
    ///     Creates verbs for all listed prototypes that match their own requirements. Uses the provided factory to create new verb instances.
    /// </summary>
    // Note: using `where T : Verb, new()` here results in a sandbox violation... Yea we peasants don't get OOP in ss14.
    private void AddAll<T>(IEnumerable<InteractionVerbPrototype> verbs, GetVerbsEvent<T> args, Func<T> factory) where T : Verb
    {
        // Don't add verbs to ghosts. Ghost system will also cancel all verbs by/on non-admin ghosts.
        if (TryComp<GhostComponent>(args.User, out var ghost) && !ghost.CanGhostInteract)
            return;

        var ownInteractions = EnsureComp<OwnInteractionVerbsComponent>(args.User);
        foreach (var proto in verbs)
        {
            DebugTools.AssertNotEqual(proto.Abstract, true, "Attempted to add a verb with an abstract prototype.");

            var name = proto.Name;
            if (!proto.AllowSelfInteract && args.User == args.Target
                || args.Verbs.Any(v => v.Text == name)
                || !Transform(args.User).Coordinates.TryDistance(EntityManager, Transform(args.Target).Coordinates, out var distance)
            )
                continue;

            var isInvalid = proto.RequiresHands && args.Hands is null
                || proto.RequiresCanInteract && !args.CanInteract
                || !proto.Range.IsInRange(distance);

            var verbArgs = InteractionArgs.From(args);
            // Calculate contest advantage early if required
            if (proto.ContestAdvantageRange is not null)
            {
                CalculateAdvantage(proto, ref verbArgs, out var canPerform);
                isInvalid |= !canPerform;
            }

            var isRequirementMet = proto.Requirement?.IsMet(verbArgs, proto, _verbDependencies) != false;
            if (!isRequirementMet && proto.HideByRequirement)
                continue;

            // TODO: we skip this check since the client is not aware of actions. This should be changed, maybe make actions mixed server/client?
            var isAllowed = proto.Action?.IsAllowed(verbArgs, proto, _verbDependencies) == true || _net.IsClient;
            if (!isAllowed && proto.HideWhenInvalid)
                continue;

            var verb = factory.Invoke();
            CopyVerbData(proto, verb);
            verb.Disabled = isInvalid || !isRequirementMet || !isAllowed;
            if (!verb.Disabled)
                verb.Act = () => StartVerb(proto, verbArgs);
            else
                verb.Message = Loc.GetString("interaction-verb-invalid");

            // This just marks the verb as disabled without removing the action so the user can still try to use it.
            if (CheckVerbCooldown(proto, verbArgs, out var remainingTime, ownInteractions))
            {
                verb.Disabled = true;
                verb.Message = Loc.GetString("interaction-verb-cooldown", ("seconds", remainingTime.TotalSeconds));
            }

            args.Verbs.Add(verb);
        }
    }

    /// <summary>
    ///     Calculates the effective contest advantage for the verb and writes their clamped value to <see cref="InteractionArgs.ContestAdvantage"/>.
    /// </summary>
    private void CalculateAdvantage(InteractionVerbPrototype proto, ref InteractionArgs args, out bool canPerform)
    {
        args.ContestAdvantage = 1f;
        canPerform = true;

        var contests = proto.AllowedContests;
        if (contests == None)
            return;

        // We don't use EveryContest here because it's straight up bad
        if (contests.HasFlag(Mass))
            args.ContestAdvantage *= _contests.MassContest(args.User, args.Target, true, 10f);
        if (contests.HasFlag(Stamina))
            args.ContestAdvantage *= _contests.MassContest(args.User, args.Target, true, 10f);
        if (contests.HasFlag(Health))
            args.ContestAdvantage *= _contests.MassContest(args.User, args.Target, true, 10f);

        canPerform = proto.ContestAdvantageRange?.IsInRange(args.ContestAdvantage.Value) ?? true;
        args.ContestAdvantage = proto.ContestAdvantageLimit.Clamp(args.ContestAdvantage.Value);
    }

    private void CopyVerbData(InteractionVerbPrototype proto, Verb verb)
    {
        verb.Text = proto.Name;
        verb.Message = proto.Description;
        verb.DoContactInteraction = proto.DoContactInteraction;
        verb.Priority = proto.Priority;
        verb.Icon = proto.Icon;
        verb.Category = VerbCategory.Interaction;
    }

    /// <summary>
    ///     Checks if the verb is on cooldown. Returns true if it still is.
    /// </summary>
    private bool CheckVerbCooldown(InteractionVerbPrototype proto, InteractionArgs args, out TimeSpan remainingTime, OwnInteractionVerbsComponent? comp = null)
    {
        remainingTime = TimeSpan.Zero;
        if (!Resolve(args.User, ref comp))
            return false;

        var cooldownTarget = proto.GlobalCooldown ? EntityUid.Invalid : args.Target;
        if (!comp.Cooldowns.TryGetValue((proto.ID, cooldownTarget), out var cooldown))
            return false;

        remainingTime = cooldown - _timing.CurTime;
        return remainingTime > TimeSpan.Zero;
    }

    private void StartVerbCooldown(InteractionVerbPrototype proto, InteractionArgs args, TimeSpan cooldown, OwnInteractionVerbsComponent? comp = null)
    {
        if (!Resolve(args.User, ref comp))
            return;

        var cooldownTarget = proto.GlobalCooldown ? EntityUid.Invalid : args.Target;
        comp.Cooldowns[(proto.ID, cooldownTarget)] = _timing.CurTime + cooldown;

        // We also clean up old cooldowns here to avoid a memory leak... This is probably a bad place to do it.
        // TODO might wanna switch to a list because dict is probably overkill for this task given we clean it up often.
        foreach (var (key, time) in comp.Cooldowns.ToArray())
        {
            if (time < _timing.CurTime)
                comp.Cooldowns.Remove(key);
        }
    }

    private void CreateVerbEffects(InteractionVerbPrototype.EffectSpecifier? specifier, InteractionPopupPrototype.Prefix prefix, InteractionVerbPrototype proto, InteractionArgs args)
    {
        // Not doing effects on client because it causes issues
        if (specifier is null || _net.IsClient)
            return;

        var (user, target, used) = (args.User, args.Target, args.Used);

        // Effect targets for different players
        var userTarget = specifier.EffectTarget is User or UserThenTarget or TargetThenUser ? user : target;
        var targetTarget = specifier.EffectTarget is Target or UserThenTarget or TargetThenUser ? target : user;
        var othersTarget = specifier.EffectTarget is Target or UserThenTarget ? target : user;
        var othersFilter = Filter.Pvs(othersTarget).RemoveWhereAttachedEntity(ent => ent == user || ent == target);

        // Popups
        if (_protoMan.TryIndex(specifier.Popup, out var popup))
        {
            var locPrefix = $"interaction-{proto.ID}-{prefix.ToString().ToLower()}";

            (string, object)[] localeArgs =
            [
                ("user", user),
                ("target", target),
                ("used", used ?? EntityUid.Invalid),
                ("selfTarget", user == target),
                ("hasUsed", used != null)
            ];

            // User popup
            var userSuffix = popup.SelfSuffix ?? popup.OthersSuffix;
            if (userSuffix is not null)
                PopupEffects(Loc.GetString($"{locPrefix}-{userSuffix}-popup", localeArgs), userTarget, Filter.Entities(user), false, popup);

            // Target popup
            var targetSuffix = popup.TargetSuffix ?? popup.OthersSuffix;
            if (targetSuffix is not null && user != target)
                PopupEffects(Loc.GetString($"{locPrefix}-{targetSuffix}-popup", localeArgs), targetTarget, Filter.Entities(target), false, popup);

            // Others popup
            var othersSuffix = popup.OthersSuffix;
            if (othersSuffix is not null)
                PopupEffects(Loc.GetString($"{locPrefix}-{othersSuffix}-popup", localeArgs), othersTarget, othersFilter, true, popup, clip: true);
        }

        // Sounds
        if (specifier.Sound is { } sound)
        {
            // TODO we have a choice between having an accurate sound source or saving on an entity spawn...
            _audio.PlayEntity(sound, Filter.Entities(user, target), target, false, specifier.SoundParams);

            if (specifier.SoundPerceivedByOthers)
                _audio.PlayEntity(sound, othersFilter, othersTarget, false, specifier.SoundParams);
        }
    }

    private void PopupEffects(string message, EntityUid target, Filter filter, bool recordReplay, InteractionPopupPrototype popup, bool clip = false)
    {
        // Sending a chat message will result in a popup anyway
        // TODO this needs to be fixed probably. Popups and chat messages should be independent.
        if (popup.LogPopup)
            SendChatLog(message, target, filter, popup, clip);
        else
            _popups.PopupEntity(message, target, filter, recordReplay, popup.PopupType);
    }

    protected virtual void SendChatLog(string message, EntityUid source, Filter filter, InteractionPopupPrototype popup, bool clip)
    {
    }

    #endregion
}
