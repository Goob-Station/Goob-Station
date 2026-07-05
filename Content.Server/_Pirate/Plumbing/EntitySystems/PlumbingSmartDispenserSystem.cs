using Content.Server._Pirate.Plumbing.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Labels.Components;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Content.Server.Hands.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles the plumbing smart dispenser: pulls all reagents from the network,
///     stores up to a per-reagent cap, supports held container dispensing and label matching dispensing
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingSmartDispenserSystem : EntitySystem
{
    private sealed class ActorUiState
    {
        public EntityUid? BoundContainer;
        public ReagentDispenserDispenseAmount DispenseAmount = ReagentDispenserDispenseAmount.U10;
    }

    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private InjectorSystem _injectorSystem = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private UserInterfaceSystem _uiSystem = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    /// <summary>
    /// Cached mapping of label prefix (lowercase) → reagent prototype ID.
    /// Built lazily on first use and invalidated on prototype reload.
    /// </summary>
    private Dictionary<string, string>? _labelCache;

    /// <summary>
    /// Cached sorted list of all reagent localized names for prefix matching.
    /// </summary>
    private List<(string LocalizedName, string PrototypeId)>? _reagentNames;

    private readonly Dictionary<(EntityUid Dispenser, EntityUid Actor), ActorUiState> _actorStates = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingSmartDispenserComponent, PlumbingDeviceUpdateEvent>(OnDeviceUpdate);
        SubscribeLocalEvent<PlumbingSmartDispenserComponent, PlumbingPullIntoAttemptEvent>(OnPullInto);
        SubscribeLocalEvent<PlumbingSmartDispenserComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<PlumbingSmartDispenserComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);

        Subs.BuiEvents<PlumbingSmartDispenserComponent>(PlumbingSmartDispenserUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnUIOpened);
            subs.Event<BoundUIClosedEvent>(OnUiClosed);
            subs.Event<PlumbingSmartDispenserRequestActorStateMessage>(OnRequestActorStateMessage);
            subs.Event<PlumbingSmartDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            subs.Event<PlumbingSmartDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            subs.Event<ReagentDispenserClearContainerSolutionMessage>(OnClearContainerMessage);
        });

        _prototypeManager.PrototypesReloaded += OnPrototypesReloaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _prototypeManager.PrototypesReloaded -= OnPrototypesReloaded;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        // Invalidate cache so it gets rebuilt with new reagent names
        _labelCache = null;
        _reagentNames = null;
    }

    private void OnDeviceUpdate(Entity<PlumbingSmartDispenserComponent> ent, ref PlumbingDeviceUpdateEvent args)
        => UpdateUiState(ent);

    /// <summary>
    /// Caps or denies pulls for reagents that are at or near the per-reagent limit.
    /// </summary>
    private void OnPullInto(Entity<PlumbingSmartDispenserComponent> ent, ref PlumbingPullIntoAttemptEvent args)
    {
        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out _, out var solution))
            return;

        var current = solution.GetReagentQuantity(new ReagentId(args.ReagentPrototype, null));
        var room = ent.Comp.MaxPerReagent - current;

        if (room <= FixedPoint2.Zero)
        {
            args.Cancelled = true;
            return;
        }

        if (room < args.MaxAllowed)
            args.MaxAllowed = room;
    }

    private void OnInteractUsing(Entity<PlumbingSmartDispenserComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<LabelComponent>(args.Used, out var label)
            && !string.IsNullOrWhiteSpace(label.CurrentLabel)
            && TryMatchLabelToReagent(label.CurrentLabel, out var reagentId))
        {
            args.Handled = true;

            TryDispenseReagent(ent, reagentId, args.Used, null, args.User, true);
            UpdateUiState(ent);
            return;
        }

        if (BuildOutputContainerInfo(args.Used) is null)
            return;

        args.Handled = true;

        var actorState = EnsureActorState(ent.Owner, args.User);
        actorState.BoundContainer = args.Used;

        _uiSystem.OpenUi(ent.Owner, PlumbingSmartDispenserUiKey.Key, args.User);
        UpdateUiState(ent);
        UpdateActorUiState(ent, args.User);
    }

    private void OnSetDispenseAmountMessage(Entity<PlumbingSmartDispenserComponent> ent, ref PlumbingSmartDispenserSetDispenseAmountMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var actorState = EnsureActorState(ent.Owner, actor);
        actorState.DispenseAmount = args.DispenseAmount;
        UpdateActorUiState(ent, actor);
    }

    private void OnRequestActorStateMessage(Entity<PlumbingSmartDispenserComponent> ent, ref PlumbingSmartDispenserRequestActorStateMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        UpdateActorUiState(ent, actor);
    }

    private void OnDispenseReagentMessage(Entity<PlumbingSmartDispenserComponent> ent, ref PlumbingSmartDispenserDispenseReagentMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var outputContainer = GetBoundContainer(ent.Owner, actor);
        if (outputContainer is not { } targetContainer)
        {
            _popup.PopupEntity(Loc.GetString("plumbing-smart-dispenser-no-container"), ent.Owner, actor);
            UpdateActorUiState(ent, actor);

            return;
        }

        var actorState = EnsureActorState(ent.Owner, actor);

        TryDispenseReagent(
            ent,
            args.ReagentId,
            targetContainer,
            FixedPoint2.New((int) actorState.DispenseAmount),
            actor,
            true);

        UpdateUiState(ent);
        UpdateActorUiState(ent, actor);
    }

    private void OnClearContainerMessage(Entity<PlumbingSmartDispenserComponent> ent, ref ReagentDispenserClearContainerSolutionMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var outputContainer = GetBoundContainer(ent.Owner, actor);
        if (outputContainer is not { } targetContainer)
        {
            UpdateActorUiState(ent, actor);
            return;
        }

        if (!_solutionSystem.TryGetFitsInDispenser(targetContainer, out var targetEnt, out _)
            && !_solutionSystem.TryGetRefillableSolution(targetContainer, out targetEnt, out _)
            && (!TryComp<InjectorComponent>(targetContainer, out var injector)
                || !TryComp<SolutionContainerManagerComponent>(targetContainer, out var manager)
                || !_solutionSystem.TryGetSolution((targetContainer, manager), injector.SolutionName, out targetEnt, out _)))
        {
            UpdateActorUiState(ent, actor);
            return;
        }

        _solutionSystem.RemoveAllSolution(targetEnt.Value);
        UpdateActorUiState(ent, actor);
    }

    private void OnUiClosed(Entity<PlumbingSmartDispenserComponent> ent, ref BoundUIClosedEvent args)
    {
        if (_actorStates.TryGetValue((ent.Owner, args.Actor), out var actorState))
            actorState.BoundContainer = null;
    }

    private void OnTerminating(Entity<PlumbingSmartDispenserComponent> ent, ref EntityTerminatingEvent args)
        => RemoveActorStatesForDispenser(ent.Owner);

    private void OnPlayerDetached(PlayerDetachedEvent args)
        => RemoveActorStatesForActor(args.Entity);

    /// <summary>
    /// Attempts to match a label string to a reagent prototype ID.
    /// Labels can contain rich text markup and abbreviations like ":[color=#ffaa00]BIC|BRT|15/5u".
    /// The algorithm strips markup, skips leading punctuation, extracts the first alphabetic token,
    /// then tries: 1) prefix match, 2) ordered subsequence match against all reagent names.
    /// Results are cached for performance.
    /// </summary>
    private bool TryMatchLabelToReagent(string label, out string reagentId)
    {
        reagentId = string.Empty;

        _labelCache ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (_labelCache.TryGetValue(label, out var cached))
        {
            reagentId = cached;
            return true;
        }

        var stripped = StripBrackets(label);
        var token = ExtractFirstAlphaToken(stripped);

        if (string.IsNullOrEmpty(token) || token.Length < 2)
            return false;

        _reagentNames ??= BuildReagentNameList();

        // Pass 1: Prefix match (covers exact matches too). Shortest name wins.
        string? bestMatch = null;
        var bestScore = int.MaxValue;

        foreach (var (name, protoId) in _reagentNames)
        {
            if (name.Length >= token.Length
                && name.StartsWith(token, StringComparison.OrdinalIgnoreCase)
                && name.Length < bestScore)
            {
                bestMatch = protoId;
                bestScore = name.Length;
            }
        }

        // Pass 2: Ordered subsequence fallback. Strip spaces from token first.
        if (bestMatch == null)
        {
            var compactToken = token.Replace(" ", "");
            bestScore = int.MaxValue;

            foreach (var (name, protoId) in _reagentNames)
            {
                var score = ScoreSubsequenceMatch(compactToken, name);
                if (score != int.MinValue && score < bestScore)
                {
                    bestMatch = protoId;
                    bestScore = score;
                }
            }
        }

        if (bestMatch == null)
            return false;

        reagentId = bestMatch;
        _labelCache[label] = reagentId;
        return true;
    }

    /// <summary>
    /// Removes all bracket-enclosed blocks (e.g. [color=#ffaa00]) from a string.
    /// </summary>
    private static string StripBrackets(string text)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        var inBracket = false;

        foreach (var c in text)
        {
            if (c == '[')
                inBracket = true;
            else if (c == ']')
                inBracket = false;
            else if (!inBracket)
                sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Scores an ordered subsequence match. Each char in token must appear in order in the name.
    /// Primary score: consecutive prefix length (more = better).
    /// Secondary: match span (compact = better). Tertiary: name length (shorter = better).
    /// Returns a score (lower = better), or -1 if no match.
    /// </summary>
    private static int ScoreSubsequenceMatch(string token, string name)
    {
        var tokenIdx = 0;
        var firstMatchPos = -1;
        var lastMatchPos = -1;
        var prefixLen = 0;
        var countingPrefix = true;

        for (var i = 0; i < name.Length && tokenIdx < token.Length; i++)
        {
            if (char.ToLowerInvariant(name[i]) == char.ToLowerInvariant(token[tokenIdx]))
            {
                if (firstMatchPos == -1)
                    firstMatchPos = i;
                lastMatchPos = i;

                if (countingPrefix)
                    prefixLen++;

                tokenIdx++;
            }
            else if (countingPrefix)
            {
                countingPrefix = false;
            }
        }

        if (tokenIdx < token.Length)
            return int.MinValue;

        return -(prefixLen * 100000) + ((lastMatchPos - firstMatchPos) * 1000) + name.Length;
    }

    /// <summary>
    /// Extracts the first run of letters (and internal spaces) from a string,
    /// skipping leading non-letter characters. Stops at non-letter, non-space characters.
    /// E.g. ":BIC|BRT" → "BIC", "dexalin plus" → "dexalin plus".
    /// </summary>
    private static string ExtractFirstAlphaToken(string text)
    {
        var start = -1;
        var end = -1;

        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsLetter(text[i]))
            {
                if (start == -1)
                    start = i;
                end = i + 1;
            }
            else if (start != -1 && text[i] == ' ')
            {
                // Allow spaces between letters
            }
            else if (start != -1)
            {
                break;
            }
        }

        if (start == -1)
            return string.Empty;

        return text[start..end].TrimEnd();
    }

    /// <summary>
    /// Builds a sorted list of (localizedName, prototypeId) for all reagent prototypes.
    /// Sorted by name length ascending so shorter (more specific) names are checked first.
    /// </summary>
    private List<(string LocalizedName, string PrototypeId)> BuildReagentNameList()
    {
        var list = new List<(string, string)>();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<ReagentPrototype>())
        {
            list.Add((proto.LocalizedName, proto.ID));
        }

        // Sort by name length so shorter matches win as tiebreaker
        list.Sort((a, b) => a.Item1.Length.CompareTo(b.Item1.Length));
        return list;
    }

    private void OnUIOpened(Entity<PlumbingSmartDispenserComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUiState(ent);
        UpdateActorUiState(ent, args.Actor);
    }

    private bool TryDispenseReagent(
        Entity<PlumbingSmartDispenserComponent> ent,
        string reagentId,
        EntityUid targetContainer,
        FixedPoint2? requestedAmount,
        EntityUid? user,
        bool showPopup)
    {
        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var sourceEnt, out var sourceSolution))
            return false;

        if (sourceEnt is not { } sourceSolutionEnt)
            return false;

        ReagentQuantity? sourceReagent = null;

        foreach (var reagent in sourceSolution.Contents)
        {
            if (reagent.Reagent.Prototype != reagentId)
                continue;

            sourceReagent = reagent;
            break;
        }

        var available = sourceReagent?.Quantity ?? FixedPoint2.Zero;

        if (available <= FixedPoint2.Zero)
        {
            if (showPopup && user is { Valid: true })
            {
                _popup.PopupEntity(
                    Loc.GetString("plumbing-smart-dispenser-not-in-stock",
                        ("reagent", _prototypeManager.Index<ReagentPrototype>(reagentId).LocalizedName)),
                    ent.Owner,
                    user.Value);
            }

            return false;
        }

        if (sourceReagent is not { } sourceReagentValue)
            return false;

        if (!_solutionSystem.TryGetFitsInDispenser(targetContainer, out var targetEnt, out var targetSolution)
            && !_solutionSystem.TryGetRefillableSolution(targetContainer, out targetEnt, out targetSolution)
            && (!TryComp<InjectorComponent>(targetContainer, out var injector)
                || !TryComp<SolutionContainerManagerComponent>(targetContainer, out var manager)
                || !_solutionSystem.TryGetSolution((targetContainer, manager), injector.SolutionName, out targetEnt, out targetSolution)))
        {
            return false;
        }

        var transferAmount = FixedPoint2.Min(available, targetSolution!.AvailableVolume);
        if (requestedAmount is { } requested)
            transferAmount = FixedPoint2.Min(transferAmount, requested);

        if (transferAmount <= FixedPoint2.Zero)
        {
            if (showPopup && user is { Valid: true })
                _popup.PopupEntity(Loc.GetString("plumbing-smart-dispenser-jug-full"), ent.Owner, user.Value);

            return false;
        }

        var removed = _solutionSystem.RemoveReagent(sourceSolutionEnt, sourceReagentValue.Reagent, transferAmount);
        if (removed <= FixedPoint2.Zero)
            return false;

        _solutionSystem.TryAddReagent(targetEnt!.Value, sourceReagentValue.Reagent, removed, out var accepted);

        if (showPopup && accepted > FixedPoint2.Zero && user is { Valid: true })
        {
            var reagentName = _prototypeManager.Index<ReagentPrototype>(reagentId).LocalizedName;
            _popup.PopupEntity(
                Loc.GetString("plumbing-smart-dispenser-filled",
                    ("reagent", reagentName),
                    ("amount", accepted)),
                ent.Owner,
                user.Value);
        }

        if (accepted > FixedPoint2.Zero
            && TryComp<InjectorComponent>(targetContainer, out var injectorComp))
        {
            if (_solutionSystem.TryGetSolution(targetContainer, injectorComp.SolutionName, out _, out var injectorSolution)
                && injectorSolution.AvailableVolume == 0
                && _prototypeManager.TryIndex(injectorComp.ActiveModeProtoId, out InjectorModePrototype? activeMode)
                && !activeMode.Behavior.HasFlag(InjectorBehavior.Inject))
            {
                SetInjectorToInjectMode((targetContainer, injectorComp), user);
            }
        }

        return accepted > FixedPoint2.Zero;
    }

    private void SetInjectorToInjectMode(Entity<InjectorComponent> injector, EntityUid? user)
    {
        foreach (var modeId in injector.Comp.AllowedModes)
        {
            if (!_prototypeManager.TryIndex(modeId, out InjectorModePrototype? mode)
                || !mode.Behavior.HasFlag(InjectorBehavior.Inject))
                continue;

            if (user is not { Valid: true } userUid)
                return;

            _injectorSystem.ToggleMode(injector, userUid, mode);
            return;
        }
    }

    private ContainerInfo? BuildOutputContainerInfo(EntityUid? container)
    {
        if (container is not { Valid: true })
            return null;

        if (!_solutionSystem.TryGetFitsInDispenser(container.Value, out _, out var solution)
            && !_solutionSystem.TryGetRefillableSolution(container.Value, out _, out solution)
            && (!TryComp<InjectorComponent>(container.Value, out var injector)
                || !TryComp<SolutionContainerManagerComponent>(container.Value, out var manager)
                || !_solutionSystem.TryGetSolution((container.Value, manager), injector.SolutionName, out _, out solution)))
        {
            return null;
        }

        return new ContainerInfo(Name(container.Value), solution!.Volume, solution.MaxVolume)
        {
            Reagents = solution.Contents,
        };
    }

    private void UpdateUiState(Entity<PlumbingSmartDispenserComponent> ent)
    {
        var entries = new List<PlumbingSmartDispenserReagentEntry>();

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out _, out var solution))
        {
            foreach (var reagent in solution.Contents)
            {
                if (reagent.Quantity <= FixedPoint2.Zero)
                    continue;

                if (!_prototypeManager.TryIndex<ReagentPrototype>(reagent.Reagent.Prototype, out var proto))
                    continue;

                entries.Add(new PlumbingSmartDispenserReagentEntry(
                    proto.ID,
                    proto.LocalizedName,
                    reagent.Quantity,
                    proto.SubstanceColor));
            }

            // Sort alphabetically by localized name for consistent display
            entries.Sort((a, b) => string.Compare(a.LocalizedName, b.LocalizedName, StringComparison.OrdinalIgnoreCase));
        }

        var state = new PlumbingSmartDispenserBuiState(
            entries,
            ent.Comp.MaxPerReagent.Float());
        _uiSystem.SetUiState(ent.Owner, PlumbingSmartDispenserUiKey.Key, state);
    }

    private void UpdateActorUiState(Entity<PlumbingSmartDispenserComponent> ent, EntityUid actor)
    {
        if (!_uiSystem.IsUiOpen(ent.Owner, PlumbingSmartDispenserUiKey.Key, actor))
            return;

        var actorState = EnsureActorState(ent.Owner, actor);
        var boundContainer = GetBoundContainer(ent.Owner, actor);
        var outputContainer = boundContainer is { } container ? BuildOutputContainerInfo(container) : null;

        if (boundContainer is { } validContainer && outputContainer is not null)
        {
            _uiSystem.ServerSendUiMessage(
                ent.Owner,
                PlumbingSmartDispenserUiKey.Key,
                new PlumbingSmartDispenserActorStateMessage(
                    true,
                    outputContainer,
                    GetNetEntity(validContainer),
                    actorState.DispenseAmount),
                actor);
            return;
        }

        actorState.BoundContainer = null;

        _uiSystem.ServerSendUiMessage(
            ent.Owner,
            PlumbingSmartDispenserUiKey.Key,
            new PlumbingSmartDispenserActorStateMessage(
                false,
                null,
                null,
                actorState.DispenseAmount),
            actor);
    }

    private ActorUiState EnsureActorState(EntityUid dispenser, EntityUid actor)
    {
        var key = (dispenser, actor);

        if (_actorStates.TryGetValue(key, out var actorState))
            return actorState;

        actorState = new ActorUiState();
        _actorStates[key] = actorState;
        return actorState;
    }

    private EntityUid? GetBoundContainer(EntityUid dispenser, EntityUid actor)
    {
        if (!_actorStates.TryGetValue((dispenser, actor), out var actorState))
            return null;

        if (actorState.BoundContainer is not { Valid: true } container)
            return null;

        if (_hands.IsHolding(actor, container, out _))
            return container;

        actorState.BoundContainer = null;
        return null;
    }

    private void RemoveActorStatesForDispenser(EntityUid dispenser)
    {
        var keys = new List<(EntityUid Dispenser, EntityUid Actor)>();

        foreach (var key in _actorStates.Keys)
        {
            if (key.Dispenser == dispenser)
                keys.Add(key);
        }

        foreach (var key in keys)
        {
            _actorStates.Remove(key);
        }
    }

    private void RemoveActorStatesForActor(EntityUid actor)
    {
        var keys = new List<(EntityUid Dispenser, EntityUid Actor)>();

        foreach (var key in _actorStates.Keys)
        {
            if (key.Actor == actor)
                keys.Add(key);
        }

        foreach (var key in keys)
        {
            _actorStates.Remove(key);
        }
    }
}
