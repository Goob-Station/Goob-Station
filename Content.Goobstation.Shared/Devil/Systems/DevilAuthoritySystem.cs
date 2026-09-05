using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Goobstation.Shared.SlaughterDemon;
using Content.Shared.Chasm;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using System;

namespace Content.Goobstation.Shared.Devil.Systems;

/// <summary>
/// fetches all slaughter demons and teleports them to the Devil.
/// Spawns a massive pool of blood under the devil regardless of it being succesfull or not. And also stuns anyone nearby.
/// </summary>
public sealed class DevilAuthoritySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly HashSet<EntityUid> _targets = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilAuthorityComponent, DevilAuthorityEvent>(OnDevilAuthorityUsed);
    }

    private void OnDevilAuthorityUsed(EntityUid uid, DevilAuthorityComponent comp, ref DevilAuthorityEvent args)
    {
        var performer = args.Performer;

        // Fetch all entities with SlaughterDemonComponent
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

        // Teleport them to performer :trolled:
        foreach (var target in _targets)
        {
            _transform.SetCoordinates(target, Transform(performer).Coordinates);
        }

        // Spawn blood puddle at performer
        Spawn(comp.BloodPuddleProto, Transform(performer).Coordinates);

        // Force all slaughter demons out of jaunt, if they are jaunting.
        foreach (var demon in _targets)
        {
            //if (HasComp<BloodCrawlComponent>(demon))
            //    RaiseLocalEvent(demon, new ForceExitBloodCrawlEvent());
        }

        // Stun all nearby entities in range
        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(performer).Coordinates, comp.StunRange, _targets);

        foreach (var target in _targets)
        {
            if (target == performer)
                continue;

            if (HasComp<SlaughterDemonComponent>(target))
                continue;

            _stun.TryAddStunDuration(target, comp.StunTime);
        }

        _audio.PlayPredicted(comp.Sound, performer, performer);
        _chat.TrySendInGameICMessage(performer, Loc.GetString(comp.Invocation), InGameICChatType.Speak, false);

        // Step 6: Profit
    }
}
