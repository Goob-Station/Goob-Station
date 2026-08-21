using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Twitch;
using Content.Server.Administration.Logs;
using Content.Server.GameTicking;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.ServerStatus;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ITwitchApiManager _twitchApi = default!;
    [Dependency] private readonly TwitchPairingSystem _pairings = default!;

    private readonly Dictionary<string, ITwitchBitsAction> _actions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PendingModeration> _pendingModeration = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProcessedTransaction> _processedTransactions = new(StringComparer.Ordinal);

    public override void Initialize()
    {
        base.Initialize();

        _twitchApi.RegisterRoute(HttpMethod.Get, "/bits/actions", HandleGetActions, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Post, "/bits/transactions", HandleTransaction, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Post, "/bits/debug/actions", HandleDebugAction, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Get, "/bits/moderation", HandleGetModeration, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Post, "/bits/moderation", HandleModerationDecision, TwitchApiAccess.ExtensionJwt);
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
        var common = GetCommonValidity(context.ChannelId, out target);
        if (!common.IsValid)
            return common;

        return action.IsCurrentlyValid(target, context);
    }

    private TwitchBitsActionValidity GetCommonValidity(string channelId, out EntityUid target)
    {
        target = EntityUid.Invalid;

        if (!_configuration.GetCVar(GoobCVars.TwitchBitsEnabled))
            return TwitchBitsActionValidity.Invalid("Bits actions are disabled on the SS14 server.");

        if (_gameTicker.RunLevel != GameRunLevel.InRound)
            return TwitchBitsActionValidity.Invalid("The station round is not currently running.");

        if (!_pairings.TryGetTargetSession(channelId, out var session, out _))
            return TwitchBitsActionValidity.Invalid("This Twitch channel has not been linked to an online SS14 account by an administrator.");

        if (session.Status != SessionStatus.InGame)
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
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var response = await _twitchApi.RunOnMainThread(() => CreateStatus(identity));
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

        var result = await _twitchApi.RunOnMainThread(() => ProcessTransaction(
            identity.ChannelId,
            transaction,
            request.Input,
            request.DisplayName));
        switch (result.Status)
        {
            case ProcessStatus.Accepted:
            case ProcessStatus.Queued:
                await context.RespondJsonAsync(new ExecuteActionResponse(
                    result.Action!.Id,
                    result.Action.DisplayName,
                    result.AlreadyProcessed,
                    result.Status == ProcessStatus.Queued,
                    result.PendingId));
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

    private async Task HandleDebugAction(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var request = await _twitchApi.ReadJsonAsync<DebugActionRequest>(context);
        if (request == null || string.IsNullOrWhiteSpace(request.ActionId))
        {
            await RespondError(context, HttpStatusCode.BadRequest, "action_required", "An action ID is required.");
            return;
        }

        var result = await _twitchApi.RunOnMainThread(() => ProcessDebugAction(
            identity.ChannelId,
            request.ActionId,
            request.Input,
            request.DisplayName,
            identity.UserId ?? identity.OpaqueUserId));
        if (result.Status is ProcessStatus.Accepted or ProcessStatus.Queued)
        {
            await context.RespondJsonAsync(new ExecuteActionResponse(
                result.Action!.Id,
                result.Action.DisplayName,
                false,
                result.Status == ProcessStatus.Queued,
                result.PendingId));
            return;
        }

        var status = result.Status == ProcessStatus.UnknownSku
            ? HttpStatusCode.BadRequest
            : HttpStatusCode.Conflict;
        await RespondError(
            context,
            status,
            result.Status == ProcessStatus.UnknownSku ? "unknown_action" : "action_unavailable",
            result.Reason ?? "That action is not currently available.");
    }

    private async Task HandleGetModeration(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        if (!CanModerate(identity.Role))
        {
            await RespondError(context, HttpStatusCode.Forbidden, "moderator_required", "A moderator, editor, or broadcaster must review text redemptions.");
            return;
        }

        var response = await _twitchApi.RunOnMainThread(() => CreateModerationStatus(identity.ChannelId));
        await context.RespondJsonAsync(response);
    }

    private async Task HandleModerationDecision(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        if (!CanModerate(identity.Role))
        {
            await RespondError(context, HttpStatusCode.Forbidden, "moderator_required", "A moderator, editor, or broadcaster must review text redemptions.");
            return;
        }

        var request = await _twitchApi.ReadJsonAsync<ModerationDecisionRequest>(context);
        if (request == null || string.IsNullOrWhiteSpace(request.Id))
        {
            await RespondError(context, HttpStatusCode.BadRequest, "redemption_required", "A pending redemption ID is required.");
            return;
        }

        var actor = identity.UserId ?? identity.OpaqueUserId;
        var result = await _twitchApi.RunOnMainThread(() => Moderate(identity.ChannelId, request.Id, request.Approve, actor));
        if (result.Status == ModerationStatus.NotFound)
        {
            await RespondError(context, HttpStatusCode.NotFound, "redemption_not_found", "That pending redemption no longer exists.");
            return;
        }

        if (result.Status == ModerationStatus.Unavailable)
        {
            await RespondError(context, HttpStatusCode.Conflict, "action_unavailable", result.Reason ?? "That action is no longer available.");
            return;
        }

        await context.RespondJsonAsync(new ModerationDecisionResponse(request.Id, request.Approve));
    }

    private ProcessResult ProcessTransaction(string channelId, TwitchBitsTransaction transaction, string? input, string? displayName)
    {
        PruneProcessedTransactions();
        PrunePendingModeration();

        if (_processedTransactions.TryGetValue(transaction.TransactionId, out var processed))
        {
            var processedAction = _actions.GetValueOrDefault(processed.ActionId);
            var pending = _pendingModeration.Values.FirstOrDefault(item => item.TransactionId == transaction.TransactionId);
            return processedAction == null
                ? new ProcessResult(ProcessStatus.UnknownSku, null, null, true)
                : new ProcessResult(
                    pending == null ? ProcessStatus.Accepted : ProcessStatus.Queued,
                    processedAction,
                    null,
                    true,
                    pending?.Id);
        }

        var matchingActions = _actions.Values
            .Where(action => string.Equals(
                action.Sku,
                transaction.Sku,
                StringComparison.Ordinal))
            .Take(2)
            .ToArray();
        if (matchingActions.Length != 1)
            return new ProcessResult(ProcessStatus.UnknownSku, null, null, false);

        var action = matchingActions[0];
        var twitchUserName = NormalizeTwitchUserName(displayName, transaction.UserId);
        var actionContext = new TwitchBitsActionContext(channelId, transaction, input, true, twitchUserName);
        var validity = IsCurrentlyValid(action, actionContext, out var target);
        if (!validity.IsValid)
            return new ProcessResult(ProcessStatus.Unavailable, action, validity.Reason, false);

        if (action.RequiresInput)
        {
            var pending = QueueModeration(
                action,
                channelId,
                input,
                twitchUserName,
                transaction.UserId,
                transaction.TransactionId,
                false);
            _processedTransactions[transaction.TransactionId] = new ProcessedTransaction(action.Id, transaction.ExpiresAt);
            _adminLogger.Add(
                LogType.Action,
                LogImpact.Medium,
                $"Twitch Bits transaction {transaction.TransactionId} from user {transaction.UserId} queued {action.Id} for moderation.");
            return new ProcessResult(ProcessStatus.Queued, action, null, false, pending.Id);
        }

        if (!action.Execute(target, actionContext))
            return new ProcessResult(ProcessStatus.Unavailable, action, "The SS14 server could not complete that action.", false);

        ShowRedemptionToast(target, twitchUserName, action.DisplayName);
        _processedTransactions[transaction.TransactionId] = new ProcessedTransaction(action.Id, transaction.ExpiresAt);
        var impact = action.Id == "arm-nuke" ? LogImpact.High : LogImpact.Medium;
        _adminLogger.Add(
            LogType.Action,
            impact,
            $"Twitch Bits transaction {transaction.TransactionId} from user {transaction.UserId} ran {action.Id} on {ToPrettyString(target)}.");
        Log.Info($"Processed Twitch Bits transaction {transaction.TransactionId}: {action.Id}");
        return new ProcessResult(ProcessStatus.Accepted, action, null, false);
    }

    private ProcessResult ProcessDebugAction(string channelId, string actionId, string? input, string? displayName, string actor)
    {
        if (!_actions.TryGetValue(actionId, out var action))
            return new ProcessResult(ProcessStatus.UnknownSku, null, "That debug action does not exist.", false);

        var twitchUserName = NormalizeTwitchUserName(displayName, actor);
        var actionContext = new TwitchBitsActionContext(channelId, null, input, true, twitchUserName);
        var validity = IsCurrentlyValid(action, actionContext, out var target);
        if (!validity.IsValid)
            return new ProcessResult(ProcessStatus.Unavailable, action, validity.Reason, false);

        if (action.RequiresInput)
        {
            var pending = QueueModeration(action, channelId, input, twitchUserName, actor, null, true);
            _adminLogger.Add(
                LogType.Action,
                LogImpact.Medium,
                $"Twitch viewer {actor} queued free debug action {action.Id} for moderation.");
            return new ProcessResult(ProcessStatus.Queued, action, null, false, pending.Id);
        }

        if (!action.Execute(target, actionContext))
            return new ProcessResult(ProcessStatus.Unavailable, action, "The SS14 server could not complete that action.", false);

        ShowRedemptionToast(target, twitchUserName, action.DisplayName);
        var impact = action.Id == "arm-nuke" ? LogImpact.High : LogImpact.Medium;
        _adminLogger.Add(
            LogType.Action,
            impact,
            $"Twitch viewer {actor} ran free debug action {action.Id} on {ToPrettyString(target)}.");
        Log.Info($"Processed free Twitch debug action from {actor}: {action.Id}");
        return new ProcessResult(ProcessStatus.Accepted, action, null, false);
    }

    private PendingModeration QueueModeration(
        ITwitchBitsAction action,
        string channelId,
        string? input,
        string twitchUserName,
        string viewerId,
        string? transactionId,
        bool debug)
    {
        var pending = new PendingModeration(
            Guid.NewGuid().ToString("N"),
            channelId,
            action.Id,
            action.DisplayName,
            input ?? string.Empty,
            twitchUserName,
            viewerId,
            transactionId,
            debug,
            DateTimeOffset.UtcNow);
        _pendingModeration.Add(pending.Id, pending);
        return pending;
    }

    private ModerationResult Moderate(string channelId, string id, bool approve, string moderator)
    {
        PrunePendingModeration();
        if (!_pendingModeration.TryGetValue(id, out var pending) || pending.ChannelId != channelId)
            return new ModerationResult(ModerationStatus.NotFound, null);

        if (!approve)
        {
            _pendingModeration.Remove(id);
            _adminLogger.Add(
                LogType.Action,
                LogImpact.Medium,
                $"Twitch moderator {moderator} rejected pending {pending.ActionId} from {pending.ViewerId}.");
            return new ModerationResult(ModerationStatus.Rejected, null);
        }

        if (!_actions.TryGetValue(pending.ActionId, out var action))
        {
            _pendingModeration.Remove(id);
            return new ModerationResult(ModerationStatus.NotFound, null);
        }

        var actionContext = new TwitchBitsActionContext(channelId, null, pending.Input, true, pending.TwitchUserName);
        var validity = IsCurrentlyValid(action, actionContext, out var target);
        if (!validity.IsValid)
            return new ModerationResult(ModerationStatus.Unavailable, validity.Reason);

        if (!action.Execute(target, actionContext))
            return new ModerationResult(ModerationStatus.Unavailable, "The SS14 server could not complete that action.");

        _pendingModeration.Remove(id);
        ShowRedemptionToast(target, pending.TwitchUserName, action.DisplayName);
        _adminLogger.Add(
            LogType.Action,
            LogImpact.Medium,
            $"Twitch moderator {moderator} approved pending {pending.ActionId} from {pending.ViewerId} on {ToPrettyString(target)}.");
        Log.Info($"Twitch moderator {moderator} approved {pending.ActionId} from {pending.ViewerId}");
        return new ModerationResult(ModerationStatus.Approved, null);
    }

    private void ShowRedemptionToast(EntityUid target, string twitchUserName, string actionName)
    {
        RaiseNetworkEvent(
            new TwitchBitsToastEvent($"{twitchUserName} redeemed {actionName}."),
            target);
    }

    private static string NormalizeTwitchUserName(string? displayName, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(displayName)
            ? fallback
            : string.Join(' ', displayName.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 25 ? normalized : normalized[..25];
    }

    private BitsStatusResponse CreateStatus(TwitchExtensionIdentity identity)
    {
        var actionResponses = new List<BitsActionResponse>(_actions.Count);
        foreach (var action in _actions.Values.OrderBy(action => action.Id, StringComparer.Ordinal))
        {
            var validity = IsCurrentlyValid(action, new TwitchBitsActionContext(identity.ChannelId, null, null), out _);
            actionResponses.Add(new BitsActionResponse(
                action.Id,
                action.Sku,
                action.DisplayName,
                action.DisplayDescription,
                action.Category,
                action.RequiresInput,
                action.MaxInputLength,
                action.InputPlaceholder,
                validity.IsValid,
                validity.Reason));
        }

        _pairings.TryGetPairing(identity.ChannelId, out var pairing);
        return new BitsStatusResponse(
            _configuration.GetCVar(GoobCVars.TwitchBitsEnabled),
            DateTimeOffset.UtcNow,
            pairing?.Ss14Username,
            CanModerate(identity.Role),
            actionResponses);
    }

    private static bool CanModerate(TwitchExtensionRole role)
    {
        return role is TwitchExtensionRole.Moderator or
            TwitchExtensionRole.Editor or
            TwitchExtensionRole.Broadcaster;
    }

    private ModerationQueueResponse CreateModerationStatus(string channelId)
    {
        PrunePendingModeration();
        var items = _pendingModeration.Values
            .Where(item => item.ChannelId == channelId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new ModerationQueueItem(
                item.Id,
                item.ActionName,
                item.Input,
                item.TwitchUserName,
                item.Debug,
                item.CreatedAt))
            .ToArray();
        return new ModerationQueueResponse(DateTimeOffset.UtcNow, items);
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

    private void PrunePendingModeration()
    {
        var cutoff = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(15);
        foreach (var (id, pending) in _pendingModeration.ToArray())
        {
            if (pending.CreatedAt < cutoff)
                _pendingModeration.Remove(id);
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
        Queued,
        UnknownSku,
        Unavailable,
    }

    private enum ModerationStatus : byte
    {
        Approved,
        Rejected,
        NotFound,
        Unavailable,
    }

    private sealed record ProcessedTransaction(string ActionId, DateTimeOffset ExpiresAt);

    private sealed record ProcessResult(
        ProcessStatus Status,
        ITwitchBitsAction? Action,
        string? Reason,
        bool AlreadyProcessed,
        string? PendingId = null);

    private sealed record PendingModeration(
        string Id,
        string ChannelId,
        string ActionId,
        string ActionName,
        string Input,
        string TwitchUserName,
        string ViewerId,
        string? TransactionId,
        bool Debug,
        DateTimeOffset CreatedAt);

    private sealed record ModerationResult(ModerationStatus Status, string? Reason);

    private sealed record ExecuteActionRequest(
        [property: JsonPropertyName("receipt")] string? Receipt,
        [property: JsonPropertyName("input")] string? Input,
        [property: JsonPropertyName("displayName")] string? DisplayName);

    private sealed record DebugActionRequest(
        [property: JsonPropertyName("actionId")] string? ActionId,
        [property: JsonPropertyName("input")] string? Input,
        [property: JsonPropertyName("displayName")] string? DisplayName);

    private sealed record ExecuteActionResponse(
        [property: JsonPropertyName("actionId")] string ActionId,
        [property: JsonPropertyName("actionName")] string ActionName,
        [property: JsonPropertyName("alreadyProcessed")] bool AlreadyProcessed,
        [property: JsonPropertyName("queuedForModeration")] bool QueuedForModeration,
        [property: JsonPropertyName("pendingId")] string? PendingId);

    private sealed record ModerationDecisionRequest(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("approve")] bool Approve);

    private sealed record ModerationDecisionResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("approved")] bool Approved);

    private sealed record ModerationQueueResponse(
        [property: JsonPropertyName("serverTime")] DateTimeOffset ServerTime,
        [property: JsonPropertyName("items")] IReadOnlyList<ModerationQueueItem> Items);

    private sealed record ModerationQueueItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("actionName")] string ActionName,
        [property: JsonPropertyName("input")] string Input,
        [property: JsonPropertyName("twitchUserName")] string TwitchUserName,
        [property: JsonPropertyName("debug")] bool Debug,
        [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt);

    private sealed record BitsStatusResponse(
        [property: JsonPropertyName("enabled")] bool Enabled,
        [property: JsonPropertyName("serverTime")] DateTimeOffset ServerTime,
        [property: JsonPropertyName("targetUsername")] string? TargetUsername,
        [property: JsonPropertyName("canModerate")] bool CanModerate,
        [property: JsonPropertyName("actions")] IReadOnlyList<BitsActionResponse> Actions);

    private sealed record BitsActionResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("sku")] string Sku,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("category")] string Category,
        [property: JsonPropertyName("requiresInput")] bool RequiresInput,
        [property: JsonPropertyName("maxInputLength")] int? MaxInputLength,
        [property: JsonPropertyName("inputPlaceholder")] string? InputPlaceholder,
        [property: JsonPropertyName("available")] bool Available,
        [property: JsonPropertyName("reason")] string? Reason);

    private sealed record ApiError(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("message")] string Message);
}
