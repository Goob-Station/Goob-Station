using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Robust.Shared.Analyzers;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.LimbGnawing;

public sealed partial class LimbGnawingSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private WoundSystem _wounds = default!;

    private static readonly BodyPartType[] AllGnawable =
        { BodyPartType.Hand, BodyPartType.Foot, BodyPartType.Arm, BodyPartType.Leg };

    [SubscribeLocalEvent]
    private void OnMobStateChanged(Entity<BodyComponent> ent, ref MobStateChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.NewMobState == MobState.Dead)
            UpdateMarker(ent);
        else if (args.OldMobState == MobState.Dead)
            RemComp<GnawableCorpseComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void OnGetVerbs(Entity<GnawableCorpseComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || !TryComp<LimbGnawerComponent>(args.User, out var gnawer)
            || !_mobState.IsDead(ent)
            || PickPart(ent, gnawer) == null)
            return;

        var user = args.User;
        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("limb-gnaw-verb"),
            Priority = 10,
            Act = () => StartGnaw(user, gnawer, target),
        });
    }

    private void StartGnaw(EntityUid user, LimbGnawerComponent gnawer, EntityUid target)
    {
        var args = new DoAfterArgs(EntityManager, user, gnawer.Delay, new LimbGnawDoAfterEvent(), user, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        if (_doAfter.TryStartDoAfter(args))
            _popup.PopupPredicted(Loc.GetString("limb-gnaw-start", ("user", user), ("target", target)), user, user);
    }

    [SubscribeLocalEvent]
    private void OnDoAfter(Entity<LimbGnawerComponent> ent, ref LimbGnawDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target)
            return;

        args.Handled = true;

        if (_net.IsClient
            || !_mobState.IsDead(target)
            || PickPart(target, ent.Comp) is not { } part
            || !TryComp<WoundableComponent>(part, out var woundable)
            || woundable.ParentWoundable is not { } parent)
            return;

        _wounds.AmputateWoundableSafely(parent, part, woundable);
        _popup.PopupEntity(Loc.GetString("limb-gnaw-finish", ("user", ent.Owner), ("part", part)), ent, PopupType.MediumCaution);

        UpdateMarker(target);
    }

    private void UpdateMarker(EntityUid body)
    {
        var hasPart = false;
        foreach (var (_, part) in _body.GetBodyChildren(body))
        {
            if (Array.IndexOf(AllGnawable, part.PartType) >= 0)
            {
                hasPart = true;
                break;
            }
        }

        if (hasPart)
            EnsureComp<GnawableCorpseComponent>(body);
        else
            RemComp<GnawableCorpseComponent>(body);
    }

    private EntityUid? PickPart(EntityUid body, LimbGnawerComponent gnawer)
    {
        var candidates = new List<EntityUid>();
        foreach (var type in gnawer.PartTypes)
        {
            foreach (var (id, part) in _body.GetBodyChildren(body))
            {
                if (part.PartType == type && TryComp<WoundableComponent>(id, out var woundable) && woundable.CanRemove)
                    candidates.Add(id);
            }

            if (candidates.Count > 0)
                return _random.Pick(candidates);
        }

        return null;
    }
}
