using Content.Goobstation.Server.Possession;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Possession;

public sealed partial class PossessionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PossessedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PossessedComponent, ComponentRemove>(OnComponentRemoved);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PossessedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.PossessionEndTime)
            {
                RemComp<PossessedComponent>(uid);
            }
        }
    }

    private void OnStartup(EntityUid uid, PossessedComponent comp, ComponentStartup args)
    {
        if (HasComp<PacifiedComponent>(uid))
        {
            comp.WasPacified = true;
            return;
        }

        EnsureComp<PacifiedComponent>(uid);

    }

    private void OnComponentRemoved(EntityUid uid, PossessedComponent comp, ComponentRemove args)
    {
        if (!comp.WasPacified)
            RemComp<PacifiedComponent>(uid);

        RevertPossession(uid, comp);
    }

    public void TryPossessTarget(NetUserId targetUserId, NetUserId possessorUserId)
    {
        if (!_mindSystem.TryGetMind(targetUserId, out var targetMind))
            return;
        if (!_mindSystem.TryGetMind(possessorUserId, out var possessorMind))
            return;

        var targetEntity = GetEntityFromMind(targetMind.Value);
        var possessorEntity = GetEntityFromMind(possessorMind.Value);

        if (targetEntity == null || possessorEntity == null)
            return;

        if (HasComp<PossessedComponent>(targetEntity.Value))
            return;

        var component = AddComp<PossessedComponent>(targetEntity.Value);
        component.OriginalMindId = targetUserId;
        component.PossessorMindId = possessorUserId;
        component.PossessorOriginalEntity = possessorEntity.Value;
        component.PossessionEndTime = _timing.CurTime + TimeSpan.FromSeconds(30);

        _mindSystem.TransferTo(possessorMind.Value, targetEntity.Value);
    }

    private void RevertPossession(EntityUid targetEntity, PossessedComponent component)
    {
        var originalMindId = component.OriginalMindId;
        var possessorMindId = component.PossessorMindId;
        var possessorOriginalEntity = component.PossessorOriginalEntity;

        // Transfer original mind back if possible
        if (_mindSystem.TryGetMind(originalMindId, out var originalMind))
        {
            if (Exists(targetEntity) && !Terminating(targetEntity))
                _mindSystem.TransferTo(originalMind.Value, targetEntity);
            else
                _mindSystem.TransferTo(originalMind.Value, null); // Become ghost if body is gone
        }

        // Transfer possessor back to their original entity or ghost
        if (_mindSystem.TryGetMind(possessorMindId, out var possessorMind))
        {
            if (Exists(possessorOriginalEntity) && !Terminating(possessorOriginalEntity))
                _mindSystem.TransferTo(possessorMind.Value, possessorOriginalEntity);
            else
                _mindSystem.TransferTo(possessorMind.Value, null);
        }
    }

    private EntityUid? GetEntityFromMind(MindComponent mind)
    {
        var query = EntityManager.EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Mind == mind.OwnedEntity)
                return uid;
        }
        return null;
    }
}
