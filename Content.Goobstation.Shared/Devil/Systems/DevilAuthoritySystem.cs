using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Goobstation.Shared.SlaughterDemon;
using Content.Shared.Chasm;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using System;

namespace Content.Goobstation.Shared.Devil.Systems;

public sealed class DevilAuthoritySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    private readonly HashSet<EntityUid> _targets = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilAuthorityComponent, DevilAuthorityEvent>(OnDevilAuthorityUsed);
    }

    private void OnDevilAuthorityUsed(EntityUid uid, DevilAuthorityComponent comp, ref DevilAuthorityEvent args)
    {
        var performer = args.Performer;

        // 1. Fetch all entities with SlaughterDemonComponent
        _targets.Clear();
        foreach (var demon in EntityQuery<SlaughterDemonComponent>())
        {
            _targets.Add(demon.Owner);
        }
        // Jaunting demons do not have that comp
        foreach (var crawler in EntityQuery<BloodCrawlComponent>())
        {
            _targets.Add(crawler.Owner);
        }

        // 2. Teleport them to performer :trolled:
        foreach (var target in _targets)
        {
            if (HasComp<TransformComponent>(target) && HasComp<TransformComponent>(performer))
            {
                Transform(target).Coordinates = Transform(performer).Coordinates;
            }
        }

        // 3. Spawn blood puddle at performer
        if (!string.IsNullOrEmpty(comp.BloodPuddleProto))
        {
            EntityManager.SpawnEntity(comp.BloodPuddleProto, Transform(performer).Coordinates);
        }

        // 4. Force all slaughter demons out of jaunt, if they are jaunting.
        foreach (var demon in _targets)
        {
            if (HasComp<BloodCrawlComponent>(demon))
                RaiseLocalEvent(demon, new ForceExitBloodCrawlEvent());
        }

        // 5. Stun all nearby entities in range
        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(performer).Coordinates, comp.StunRange, _targets);

        foreach (var target in _targets)
        {
            if (target == performer)
                continue;

            if (HasComp<SlaughterDemonComponent>(target))
                continue;

            _stun.TryParalyze(target, comp.StunTime, true);
        }

        _audio.PlayPredicted(comp.Sound, performer, performer);
        _chat.TrySendInGameICMessage(performer, Loc.GetString(comp.Invocation), InGameICChatType.Speak, false);

        // Step 6: Profit
    }
}
