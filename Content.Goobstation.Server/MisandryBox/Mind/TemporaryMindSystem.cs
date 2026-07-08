using Content.Goobstation.Shared.MisandryBox.Mind;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.MisandryBox.Mind;

/// <summary>
/// Manages temporary mind swaps: shelves a player's real mind and gives them a fresh
/// disposable one for a temporary body. On cleanup the disposable mind is deleted
/// and the player reconnects to their original mind, preserving all roles and objectives.
/// </summary>
public sealed class TemporaryMindSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporaryMindComponent, PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerDetached(Entity<TemporaryMindComponent> ent, ref PlayerDetachedEvent args)
    {
        if (args.Player.Status != SessionStatus.Disconnected)
            return;

        CleanupDisposableMind(ent.Comp);
        RemComp<TemporaryMindComponent>(ent);
    }

    /// <summary>
    /// Shelves the player's current mind and creates a fresh disposable one for the new body.
    /// </summary>
    public bool TrySwapTempMind(ICommonSession session, EntityUid newBody)
    {
        if (session.AttachedEntity is not { Valid: true } currentEntity)
            return false;

        if (!_mind.TryGetMind(currentEntity, out var origMindId, out var origMind))
            return false;

        var userId = origMind.UserId;
        _mind.UnVisit(origMindId, origMind);

        // Clear userId from original mind so the disposable mind can claim it in UserMinds
        _mind.SetUserId(origMindId, null, origMind);

        // Suppress catatonic examine on the original body while mind is swapped
        if (origMind.OwnedEntity is { } originalBody
            && TryComp<MindContainerComponent>(originalBody, out var mindContainer))
            _mind.SetShowExamineInfo((originalBody, mindContainer), false);

        var newMind = _mind.CreateMind(userId, origMind.CharacterName);
        _mind.TransferTo(newMind, newBody);
        _playerManager.SetAttachedEntity(session, newBody);

        var comp = EnsureComp<TemporaryMindComponent>(newBody);
        comp.OriginalMind = origMindId;
        comp.DisposableMind = newMind;

        return true;
    }

    /// <summary>
    /// Discards the disposable mind and restores the original mind as a ghost at the body's location.
    /// </summary>
    public EntityUid? TryRestoreAsGhost(EntityUid temporaryBody)
    {
        if (!TryComp<TemporaryMindComponent>(temporaryBody, out var temp))
            return null;

        if (!TryComp<MindComponent>(temp.OriginalMind, out var origMind))
            return null;

        var coords = _transform.GetMapCoordinates(temporaryBody);

        CleanupDisposableMind(temp);

        var ghost = Spawn("MobObserver", coords);
        _mind.Visit(temp.OriginalMind, ghost, origMind);

        var canReturn = origMind.OwnedEntity is { } ownedEntity && Exists(ownedEntity);

        if (TryComp<GhostComponent>(ghost, out var ghostComp))
            _ghost.SetCanReturnToBody((ghost, ghostComp), canReturn);

        if (!string.IsNullOrWhiteSpace(origMind.CharacterName))
            _meta.SetEntityName(ghost, FormattedMessage.EscapeText(origMind.CharacterName));
        else if (origMind.UserId is { } userId && _playerManager.TryGetSessionById(userId, out var session))
            _meta.SetEntityName(ghost, FormattedMessage.EscapeText(session.Name));

        RemComp<TemporaryMindComponent>(temporaryBody);
        return ghost;
    }

    /// <summary>
    /// Discards the disposable mind and restores the original mind to its owned body.
    /// </summary>
    public bool TryRestoreToOriginalBody(EntityUid temporaryBody)
    {
        if (!TryComp<TemporaryMindComponent>(temporaryBody, out var temp))
            return false;

        if (!TryComp<MindComponent>(temp.OriginalMind, out var origMind))
            return false;

        var originalBody = origMind.OwnedEntity;
        if (originalBody == null || !Exists(originalBody))
            return false;

        CleanupDisposableMind(temp);

        if (origMind.UserId is { } userId && _playerManager.TryGetSessionById(userId, out var session))
            _playerManager.SetAttachedEntity(session, originalBody.Value);

        RemComp<TemporaryMindComponent>(temporaryBody);
        return true;
    }

    private void CleanupDisposableMind(TemporaryMindComponent temp)
    {
        if (Exists(temp.DisposableMind))
        {
            _mind.WipeMind(temp.DisposableMind);
            QueueDel(temp.DisposableMind);
        }

        if (TryComp<MindComponent>(temp.OriginalMind, out var origMind)
            && origMind.OriginalOwnerUserId is { } userId)
        {
            _mind.SetUserId(temp.OriginalMind, userId);

            if (origMind.OwnedEntity is { } originalBody
                && TryComp<MindContainerComponent>(originalBody, out var mindContainer))
                _mind.SetShowExamineInfo((originalBody, mindContainer), true);
        }
    }
}
