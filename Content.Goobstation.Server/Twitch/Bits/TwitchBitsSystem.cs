using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Server.Administration.Logs;
using Content.Server.GameTicking;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.Player;
using Robust.Server.ServerStatus;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ITwitchApiManager _twitchApi = default!;

    private readonly Dictionary<string, ITwitchBitsAction> _actions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProcessedTransaction> _processedTransactions = new(StringComparer.Ordinal);

    public override void Initialize()
    {
        base.Initialize();

        _twitchApi.RegisterRoute(HttpMethod.Get, "/bits/actions", HandleGetActions, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Post, "/bits/transactions", HandleTransaction, TwitchApiAccess.ExtensionJwt);
    }

    public void RegisterAction(ITwitchBitsAction action)
    {
        if (!_actions.TryAdd(action.Id, action))
            throw new InvalidOperationException($"A Twitch Bits action with ID {action.Id} is already registered.");
    }

    public TwitchBitsActionValidity IsCurrentlyValid(
        ITwitchBitsAction action,
        TwitchBitsActionContext context,
        out EntityUid target)
    {
        var common = GetCommonValidity(out target);
        if (!common.IsValid)
            return common;

        return action.IsCurrentlyValid(target, context);
    }

    private TwitchBitsActionValidity GetCommonValidity(out EntityUid target)
    {
        target = EntityUid.Invalid;

        if (!_configuration.GetCVar(GoobCVars.TwitchBitsEnabled))
            return TwitchBitsActionValidity.Invalid("Bits actions are disabled on the SS14 server.");

        if (_gameTicker.RunLevel != GameRunLevel.InRound)
            return TwitchBitsActionValidity.Invalid("The station round is not currently running.");

        var username = _configuration.GetCVar(GoobCVars.TwitchBitsTargetUsername).Trim();
        if (string.IsNullOrEmpty(username))
            return TwitchBitsActionValidity.Invalid("The streamer has not configured an SS14 target.");

        if (!_playerManager.TryGetSessionByUsername(username, out var session) ||
            session.Status != SessionStatus.InGame)
        {
            return TwitchBitsActionValidity.Invalid("The streamer is not currently in game.");
        }

        if (session.AttachedEntity is not { Valid: true } attached || !Exists(attached))
            return TwitchBitsActionValidity.Invalid("The streamer does not have an active character.");

        if (HasComp<GhostComponent>(attached))
            return TwitchBitsActionValidity.Invalid("The streamer is currently an observer.");

        if (!TryComp<MobStateComponent>(attached, out var mobState) || mobState.CurrentState != MobState.Alive)
            return TwitchBitsActionValidity.Invalid("The streamer's character is not alive.");

        target = attached;
        return TwitchBitsActionValidity.Valid;
    }

    private async Task HandleGetActions(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out _))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var response = await _twitchApi.RunOnMainThread(CreateStatus);
        await context.RespondJsonAsync(response);
    }

    private async Task HandleTransaction(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var request = await _twitchApi.ReadJsonAsync<ExecuteActionRequest>(context);
        if (request == null || string.IsNullOrWhiteSpace(request.Receipt))
        {
            await RespondError(context, HttpStatusCode.BadRequest, "receipt_required", "A Twitch transaction receipt is required.");
            return;
        }

        if (!_twitchApi.TryValidateBitsReceipt(
                request.Receipt,
                DateTimeOffset.UtcNow,
                out var transaction,
                out var validationError))
        {
            var status = validationError == TwitchBitsReceiptValidationError.MissingConfiguration
                ? HttpStatusCode.ServiceUnavailable
                : HttpStatusCode.BadRequest;
            await RespondError(context, status, "invalid_receipt", "The Twitch transaction receipt is invalid or expired.");
            return;
        }

        if (!identity.IsLinked || string.IsNullOrEmpty(identity.UserId))
        {
            await RespondError(
                context,
                HttpStatusCode.Conflict,
                "identity_required",
                "Waiting for Twitch to refresh the viewer identity after the purchase.");
            return;
        }

        if (!string.Equals(identity.UserId, transaction.UserId, StringComparison.Ordinal))
        {
            await RespondError(
                context,
                HttpStatusCode.Forbidden,
                "receipt_identity_mismatch",
                "The transaction receipt belongs to a different Twitch viewer.");
            return;
        }

        var result = await _twitchApi.RunOnMainThread(() => ProcessTransaction(transaction, request.Input));
        switch (result.Status)
        {
            case ProcessStatus.Accepted:
                await context.RespondJsonAsync(new ExecuteActionResponse(
                    result.Action!.Id,
                    result.Action.DisplayName,
                    result.AlreadyProcessed));
                return;
            case ProcessStatus.UnknownSku:
                await RespondError(
                    context,
                    HttpStatusCode.BadRequest,
                    "unknown_sku",
                    "That Twitch product is not mapped to an SS14 action.");
                return;
            default:
                await RespondError(
                    context,
                    HttpStatusCode.Conflict,
                    "action_unavailable",
                    result.Reason ?? "That action is not currently available.");
                return;
        }
    }

    private ProcessResult ProcessTransaction(TwitchBitsTransaction transaction, string? input)
    {
        PruneProcessedTransactions();

        if (_processedTransactions.TryGetValue(transaction.TransactionId, out var processed))
        {
            var processedAction = _actions.GetValueOrDefault(processed.ActionId);
            return processedAction == null
                ? new ProcessResult(ProcessStatus.UnknownSku, null, null, true)
                : new ProcessResult(ProcessStatus.Accepted, processedAction, null, true);
        }

        var matchingActions = _actions.Values
            .Where(action => string.Equals(
                _configuration.GetCVar(action.Sku).Trim(),
                transaction.Sku,
                StringComparison.Ordinal))
            .Take(2)
            .ToArray();
        if (matchingActions.Length != 1)
            return new ProcessResult(ProcessStatus.UnknownSku, null, null, false);

        var action = matchingActions[0];
        var actionContext = new TwitchBitsActionContext(transaction, input);
        var validity = IsCurrentlyValid(action, actionContext, out var target);
        if (!validity.IsValid)
            return new ProcessResult(ProcessStatus.Unavailable, action, validity.Reason, false);

        if (!action.Execute(target, actionContext))
            return new ProcessResult(ProcessStatus.Unavailable, action, "The SS14 server could not complete that action.", false);

        _processedTransactions[transaction.TransactionId] = new ProcessedTransaction(action.Id, transaction.ExpiresAt);
        var impact = action.Id == "arm-nuke" ? LogImpact.High : LogImpact.Medium;
        _adminLogger.Add(
            LogType.Action,
            impact,
            $"Twitch Bits transaction {transaction.TransactionId} from user {transaction.UserId} ran {action.Id} on {ToPrettyString(target)}.");
        Log.Info($"Processed Twitch Bits transaction {transaction.TransactionId}: {action.Id}");
        return new ProcessResult(ProcessStatus.Accepted, action, null, false);
    }

    private BitsStatusResponse CreateStatus()
    {
        var actionResponses = new List<BitsActionResponse>(_actions.Count);
        foreach (var action in _actions.Values.OrderBy(action => action.Id, StringComparer.Ordinal))
        {
            var validity = IsCurrentlyValid(action, new TwitchBitsActionContext(null, null), out _);
            actionResponses.Add(new BitsActionResponse(
                action.Id,
                _configuration.GetCVar(action.Sku).Trim(),
                action.DisplayName,
                action.DisplayDescription,
                action.RequiresInput,
                action.MaxInputLength,
                action.InputPlaceholder,
                validity.IsValid,
                validity.Reason));
        }

        var target = _configuration.GetCVar(GoobCVars.TwitchBitsTargetUsername).Trim();
        return new BitsStatusResponse(
            _configuration.GetCVar(GoobCVars.TwitchBitsEnabled),
            DateTimeOffset.UtcNow,
            string.IsNullOrEmpty(target) ? null : target,
            actionResponses);
    }

    private void PruneProcessedTransactions()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (transactionId, transaction) in _processedTransactions.ToArray())
        {
            if (transaction.ExpiresAt <= now)
                _processedTransactions.Remove(transactionId);
        }
    }

    private static Task RespondError(
        IStatusHandlerContext context,
        HttpStatusCode statusCode,
        string error,
        string message)
    {
        return context.RespondJsonAsync(new ApiError(error, message), statusCode);
    }

    private enum ProcessStatus : byte
    {
        Accepted,
        UnknownSku,
        Unavailable,
    }

    private sealed record ProcessedTransaction(string ActionId, DateTimeOffset ExpiresAt);

    private sealed record ProcessResult(
        ProcessStatus Status,
        ITwitchBitsAction? Action,
        string? Reason,
        bool AlreadyProcessed);

    private sealed record ExecuteActionRequest(
        [property: JsonPropertyName("receipt")] string? Receipt,
        [property: JsonPropertyName("input")] string? Input);

    private sealed record ExecuteActionResponse(
        [property: JsonPropertyName("actionId")] string ActionId,
        [property: JsonPropertyName("actionName")] string ActionName,
        [property: JsonPropertyName("alreadyProcessed")] bool AlreadyProcessed);

    private sealed record BitsStatusResponse(
        [property: JsonPropertyName("enabled")] bool Enabled,
        [property: JsonPropertyName("serverTime")] DateTimeOffset ServerTime,
        [property: JsonPropertyName("targetUsername")] string? TargetUsername,
        [property: JsonPropertyName("actions")] IReadOnlyList<BitsActionResponse> Actions);

    private sealed record BitsActionResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("sku")] string Sku,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("requiresInput")] bool RequiresInput,
        [property: JsonPropertyName("maxInputLength")] int? MaxInputLength,
        [property: JsonPropertyName("inputPlaceholder")] string? InputPlaceholder,
        [property: JsonPropertyName("available")] bool Available,
        [property: JsonPropertyName("reason")] string? Reason);

    private sealed record ApiError(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("message")] string Message);
}
