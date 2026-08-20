using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Server.Administration.Logs;
using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Robust.Server.ServerStatus;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Twitch.Secret;

public sealed class TwitchSecretSystem : GameRuleSystem<TwitchSecretRuleComponent>
{
    private static readonly TimeSpan VoteDuration = TimeSpan.FromSeconds(60);

    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly EventManagerSystem _eventManager = default!;
    [Dependency] private readonly ITwitchApiManager _twitchApi = default!;

    private TwitchEventVote? _activeVote;
    private CompletedVote? _lastResult;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationEventSelectionAttemptEvent>(OnEventSelectionAttempt);
        _twitchApi.RegisterRoute(HttpMethod.Get, "/votes/current", HandleGetCurrentVote, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Post, "/votes/current", HandleCastVote, TwitchApiAccess.ExtensionJwt);
    }

    protected override void Started(
        EntityUid uid,
        TwitchSecretRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        _activeVote = null;
        _lastResult = null;
        Log.Info("Twitch Secret event voting enabled for this round.");
    }

    protected override void Ended(
        EntityUid uid,
        TwitchSecretRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleEndedEvent args)
    {
        if (_activeVote != null)
            Log.Info($"Cancelled Twitch event vote {_activeVote.Id} because Twitch Secret ended.");

        _activeVote = null;
        _lastResult = null;
    }

    protected override void ActiveTick(
        EntityUid uid,
        TwitchSecretRuleComponent component,
        GameRuleComponent gameRule,
        float frameTime)
    {
        if (_activeVote != null && Timing.RealTime >= _activeVote.EndRealTime)
            CompleteVote();
    }

    private void OnEventSelectionAttempt(ref StationEventSelectionAttemptEvent args)
    {
        if (!IsTwitchSecretActive())
            return;

        args.Handled = true;

        if (_activeVote != null)
        {
            args.ConsumeSchedule = false;
            return;
        }

        if (args.Candidates.Count < 3)
        {
            args.ConsumeSchedule = false;
            return;
        }

        var remaining = args.Candidates.ToDictionary(pair => pair.Key, pair => pair.Value);
        var options = new List<TwitchEventVoteOption>(3);
        for (var i = 0; i < 3; i++)
        {
            var eventId = _eventManager.FindEvent(remaining);
            if (eventId == null)
            {
                args.ConsumeSchedule = false;
                return;
            }

            var prototype = remaining.Keys.First(proto => proto.ID == eventId);
            options.Add(new TwitchEventVoteOption(i.ToString(), prototype.ID, GetEventDisplayName(prototype)));
            remaining.Remove(prototype);
        }

        var openedAt = DateTimeOffset.UtcNow;
        _activeVote = new TwitchEventVote(
            Guid.NewGuid().ToString("N"),
            Timing.RealTime + VoteDuration,
            openedAt,
            openedAt + VoteDuration,
            options);
        args.ConsumeSchedule = true;

        var optionNames = string.Join(", ", options.Select(option => $"{option.PublicId}:{option.Name}"));
        _adminLogger.Add(
            LogType.EventStarted,
            LogImpact.Medium,
            $"Twitch Secret opened vote {_activeVote.Id} with options {optionNames}.");
        Log.Info($"Opened Twitch event vote {_activeVote.Id}: {optionNames}");
    }

    private static string GetEventDisplayName(EntityPrototype prototype)
    {
        if (!string.IsNullOrWhiteSpace(prototype.Name))
            return prototype.Name;

        var name = CaseConversion.PascalToKebab(prototype.ID).Replace('-', ' ');
        if (name.EndsWith(" rule", StringComparison.Ordinal))
            name = name[..^5];

        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private bool IsTwitchSecretActive()
    {
        var query = EntityQueryEnumerator<TwitchSecretRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out _, out var gameRule))
        {
            if (GameTicker.IsGameRuleActive(uid, gameRule))
                return true;
        }

        return false;
    }

    private void CompleteVote()
    {
        var vote = _activeVote;
        if (vote == null)
            return;

        _activeVote = null;

        var currentlyAvailable = _eventManager.AvailableEvents();
        var validOptions = vote.Options
            .Where(option => currentlyAvailable.Keys.Any(proto => proto.ID == option.EventPrototypeId))
            .ToList();

        if (validOptions.Count == 0)
        {
            Log.Warning($"Twitch event vote {vote.Id} ended without any options that were still valid.");
            _lastResult = new CompletedVote(vote.Id, DateTimeOffset.UtcNow, null, null, vote.Votes.Count);
            return;
        }

        var highestVotes = validOptions.Max(option => option.VoteCount);
        var tied = validOptions.Where(option => option.VoteCount == highestVotes).ToList();
        var winner = RobustRandom.Pick(tied);

        _lastResult = new CompletedVote(
            vote.Id,
            DateTimeOffset.UtcNow,
            winner.PublicId,
            winner.Name,
            vote.Votes.Count);

        _adminLogger.Add(
            LogType.EventStarted,
            LogImpact.Medium,
            $"Twitch Secret vote {vote.Id} selected {winner.EventPrototypeId} with {winner.VoteCount} of {vote.Votes.Count} votes.");
        Log.Info(
            $"Twitch event vote {vote.Id} selected {winner.EventPrototypeId} with {winner.VoteCount} of {vote.Votes.Count} votes.");
        _eventManager.RunNamedEvent(winner.EventPrototypeId);
    }

    private async Task HandleGetCurrentVote(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var response = await _twitchApi.RunOnMainThread(() => CreateStatus(identity.OpaqueUserId));
        await context.RespondJsonAsync(response);
    }

    private async Task HandleCastVote(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var request = await _twitchApi.ReadJsonAsync<CastVoteRequest>(context);
        if (request == null || string.IsNullOrWhiteSpace(request.VoteId) || string.IsNullOrWhiteSpace(request.OptionId))
        {
            await context.RespondJsonAsync(
                new ApiError("invalid_vote", "voteId and optionId are required."),
                HttpStatusCode.BadRequest);
            return;
        }

        var result = await _twitchApi.RunOnMainThread(
            () => CastVote(identity.OpaqueUserId, request.VoteId, request.OptionId));

        switch (result.Status)
        {
            case CastVoteStatus.Accepted:
                await context.RespondJsonAsync(result.Response!);
                return;
            case CastVoteStatus.InvalidOption:
                await context.RespondJsonAsync(
                    new ApiError("invalid_option", "The selected option is not part of the active vote."),
                    HttpStatusCode.BadRequest);
                return;
            default:
                await context.RespondJsonAsync(
                    new ApiError("vote_not_active", "That Twitch event vote is no longer active."),
                    HttpStatusCode.Conflict);
                return;
        }
    }

    private CastVoteResult CastVote(string viewerId, string voteId, string optionId)
    {
        var vote = _activeVote;
        if (vote == null || vote.Id != voteId)
            return new CastVoteResult(CastVoteStatus.NotActive, null);

        if (Timing.RealTime >= vote.EndRealTime)
        {
            CompleteVote();
            return new CastVoteResult(CastVoteStatus.NotActive, null);
        }

        var selected = vote.Options.FirstOrDefault(option => option.PublicId == optionId);
        if (selected == null)
            return new CastVoteResult(CastVoteStatus.InvalidOption, null);

        if (vote.Votes.TryGetValue(viewerId, out var previousOptionId))
        {
            if (previousOptionId == optionId)
                return new CastVoteResult(CastVoteStatus.Accepted, CreateStatus(viewerId));

            var previous = vote.Options.First(option => option.PublicId == previousOptionId);
            previous.VoteCount--;
        }

        vote.Votes[viewerId] = optionId;
        selected.VoteCount++;
        return new CastVoteResult(CastVoteStatus.Accepted, CreateStatus(viewerId));
    }

    private VoteStatusResponse CreateStatus(string viewerId)
    {
        ActiveVoteResponse? active = null;
        if (_activeVote != null)
        {
            _activeVote.Votes.TryGetValue(viewerId, out var viewerVote);
            active = new ActiveVoteResponse(
                _activeVote.Id,
                _activeVote.OpenedAt,
                _activeVote.EndsAt,
                _activeVote.Votes.Count,
                viewerVote,
                _activeVote.Options
                    .Select(option => new VoteOptionResponse(option.PublicId, option.Name, option.VoteCount))
                    .ToArray());
        }

        CompletedVoteResponse? completed = null;
        if (_lastResult != null)
        {
            completed = new CompletedVoteResponse(
                _lastResult.VoteId,
                _lastResult.CompletedAt,
                _lastResult.WinnerOptionId,
                _lastResult.WinnerName,
                _lastResult.TotalVotes);
        }

        return new VoteStatusResponse(active != null, DateTimeOffset.UtcNow, active, completed);
    }

    private sealed class TwitchEventVote(
        string id,
        TimeSpan endRealTime,
        DateTimeOffset openedAt,
        DateTimeOffset endsAt,
        List<TwitchEventVoteOption> options)
    {
        public string Id { get; } = id;
        public TimeSpan EndRealTime { get; } = endRealTime;
        public DateTimeOffset OpenedAt { get; } = openedAt;
        public DateTimeOffset EndsAt { get; } = endsAt;
        public List<TwitchEventVoteOption> Options { get; } = options;
        public Dictionary<string, string> Votes { get; } = new(StringComparer.Ordinal);
    }

    private sealed class TwitchEventVoteOption(string publicId, string eventPrototypeId, string name)
    {
        public string PublicId { get; } = publicId;
        public string EventPrototypeId { get; } = eventPrototypeId;
        public string Name { get; } = name;
        public int VoteCount { get; set; }
    }

    private sealed record CompletedVote(
        string VoteId,
        DateTimeOffset CompletedAt,
        string? WinnerOptionId,
        string? WinnerName,
        int TotalVotes);

    private enum CastVoteStatus : byte
    {
        Accepted,
        NotActive,
        InvalidOption,
    }

    private sealed record CastVoteResult(CastVoteStatus Status, VoteStatusResponse? Response);

    private sealed record CastVoteRequest(
        [property: JsonPropertyName("voteId")] string? VoteId,
        [property: JsonPropertyName("optionId")] string? OptionId);

    private sealed record VoteStatusResponse(
        [property: JsonPropertyName("active")] bool Active,
        [property: JsonPropertyName("serverTime")] DateTimeOffset ServerTime,
        [property: JsonPropertyName("vote")] ActiveVoteResponse? Vote,
        [property: JsonPropertyName("lastResult")] CompletedVoteResponse? LastResult);

    private sealed record ActiveVoteResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("openedAt")] DateTimeOffset OpenedAt,
        [property: JsonPropertyName("endsAt")] DateTimeOffset EndsAt,
        [property: JsonPropertyName("totalVotes")] int TotalVotes,
        [property: JsonPropertyName("viewerVote")] string? ViewerVote,
        [property: JsonPropertyName("options")] IReadOnlyList<VoteOptionResponse> Options);

    private sealed record VoteOptionResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("votes")] int Votes);

    private sealed record CompletedVoteResponse(
        [property: JsonPropertyName("voteId")] string VoteId,
        [property: JsonPropertyName("completedAt")] DateTimeOffset CompletedAt,
        [property: JsonPropertyName("winnerOptionId")] string? WinnerOptionId,
        [property: JsonPropertyName("winnerName")] string? WinnerName,
        [property: JsonPropertyName("totalVotes")] int TotalVotes);

    private sealed record ApiError(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("message")] string Message);
}
