using System.Linq;
using Content.Goobstation.Shared.Werewolf.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// Cuts off a random limb if user is attacking the werewolf unarmed
/// </summary>
public sealed class WerewolfCounterSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handSystem = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _woundSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private EntityQuery<BodyComponent> _bodyQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        _bodyQuery = GetEntityQuery<BodyComponent>();

        SubscribeLocalEvent<WerewolfCounterComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    private void OnBeforeDamageChanged(Entity<WerewolfCounterComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Origin == null
            || !_bodyQuery.TryComp(args.Origin, out var bodyComponent)
            || HasComp<WerewolfComponent>(args.Origin))
            return;

        var hand = _handSystem.GetActiveHand(args.Origin.Value);

        if (hand == null || !hand.IsEmpty)
            return;

        RipLimb(args.Origin.Value, bodyComponent);
        args.Cancelled = true;
    }

    private void RipLimb(EntityUid target, BodyComponent body)
    {
        var hands = _body.GetBodyChildrenOfType(target, BodyPartType.Hand, body).ToList();

        if (hands.Count <= 0)
            return;

        var pick = _random.Pick(hands);

        if (!TryComp<WoundableComponent>(pick.Id, out var woundable)
            || !woundable.ParentWoundable.HasValue)
            return;

        _woundSystem.AmputateWoundableSafely(woundable.ParentWoundable.Value, pick.Id, woundable);
    }
}
