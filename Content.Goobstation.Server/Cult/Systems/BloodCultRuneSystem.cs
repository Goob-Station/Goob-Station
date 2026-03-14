using Robust.Shared.Player;
using Content.Shared.Chat;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using System.Linq;
using Content.Goobstation.Shared.Cult.Runes;
using Content.Goobstation.Shared.Cult;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Humanoid;

namespace Content.Goobstation.Server.Cult.Systems;

public sealed partial class BloodCultRuneSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuneComponent, InteractHandEvent>(OnIntearctHand);
        SubscribeLocalEvent<BloodCultRuneComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<BloodCultRuneComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !HasComp<BloodCultistComponent>(args.Examiner) && !HasComp<GhostComponent>(args.Examiner))
            return;

        var names = string.Empty;
        var descriptions = string.Empty;
        for (int i = 0; i < ent.Comp.Behaviors!.Count; i++)
        {
            var ev = ent.Comp.Behaviors[i];
            var name = Loc.GetString(ev.InspectNameLoc);
            var desc = Loc.GetString(ev.InspectDescLoc);
            var color = ev.PulseColor.ToHex();

            // this is a name1 | name2 | name3 rune.
            names += $"{((i > 0) ? " | " : "")}[color={color}]{name}[/color]";

            // "- description 1"
            // "\n- description 2"
            descriptions += $"\n- [color={color}]{desc}[/color]";
        }

        args.PushMarkup(Loc.GetString("rune-examine", ("name", names), ("rituals", descriptions)));
    }

    private void OnIntearctHand(Entity<BloodCultRuneComponent> ent, ref InteractHandEvent args)
    {
        var user = args.User;

        if (!HasComp<BloodCultistComponent>(user))
            return;

        var invokersLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.InvokersLookupRange).ToList();
        invokersLookup = invokersLookup.Where(q => q != user).ToList();
        invokersLookup = invokersLookup.Where(q => HasComp<BloodCultistComponent>(q)).ToList();

        // user is always the first invoker.
        var invokers = new List<EntityUid> { user };
        if (HasComp<BloodCultistLeaderComponent>(user))
            invokers.Add(user); // Leaders counts as 2. Also gets twice as much i guess.

        foreach (var invoker in invokersLookup)
        {
            invokers.Add(invoker);
            if (HasComp<BloodCultistLeaderComponent>(invoker))
                invokers.Add(invoker); // if leader is not the user edge case
        }

        var targetsLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.TargetsLookupRange)
            .Where(q => HasComp<HumanoidAppearanceComponent>(q))
            .Where(q => !HasComp<BloodCultistComponent>(q))
            .ToList();

        InvokeRune(ent, invokers, targetsLookup);

        args.Handled = true;
    }

    public void InvokeRune(Entity<BloodCultRuneComponent> ent, List<EntityUid> invokers, List<EntityUid> targets)
    {
        var invoker = invokers.First();
        var behaviors = new List<(bool Valid, CultRuneBehavior Behavior, string Loc)>();
        foreach (var behavior in ent.Comp.Behaviors!)
            behaviors.Add((behavior.IsValid(EntityManager, invokers, targets, out var loc), behavior, loc));

        if (behaviors.All(q => !q.Valid))
        {
            var first = behaviors.First();

            if (!string.IsNullOrWhiteSpace(first.Loc))
                _popup.PopupEntity(first.Loc, invoker, invoker);

            return;
        }

        var valid = behaviors.Where(q => q.Valid).First();
        valid.Behavior.Invoke(EntityManager, invokers, targets, invoker);

        foreach (var i in invokers.Distinct().ToList())
            ProcessInvoker(i, valid.Behavior);


        _colorFlash.RaiseEffect(Color.Red, [ent], Filter.Pvs(ent, entityManager: EntityManager));
    }

    private void ProcessInvoker(EntityUid invoker, CultRuneBehavior? behavior)
    {
        if (behavior == null) return;

        var message = Loc.GetString(behavior.InvokeLoc);
        _chat.TrySendInGameICMessage(invoker, message, InGameICChatType.Speak, true, hideLog: true);

        // todo make it get the hand used on the rune
        if (behavior.Damage != null)
            _dmg.TryChangeDamage(invoker, behavior.Damage, true, targetPart: TargetBodyPart.Hands);
    }
}
