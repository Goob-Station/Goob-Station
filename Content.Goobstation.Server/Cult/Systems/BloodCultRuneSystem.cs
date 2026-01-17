using Robust.Shared.Player;
using Content.Shared.Chat;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using System.Linq;
using Content.Goobstation.Shared.Cult.Runes;
using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Events;
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
        SubscribeRunes();
    }

    private void OnExamined(Entity<BloodCultRuneComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !HasComp<BloodCultistComponent>(args.Examiner) && !HasComp<GhostComponent>(args.Examiner))
            return;

        var names = string.Empty;
        var descriptions = string.Empty;
        for (int i = 0; i < ent.Comp.Events.Count; i++)
        {
            var ev = ent.Comp.Events[i];
            var name = Loc.GetString(ev.InspectNameLoc);
            var desc = Loc.GetString(ev.InspectDescLoc);
            var color = ev.PulseColor.ToHex();

            // "name1 / name2 / name3"
            names += $"{((i > 0) ? " \\ " : "")}[color={color}]{name}[/color]";

            // "- description 1"
            // "\n- description 2"
            descriptions += $"\n- [color={color}]{desc}[/color]";
        }

        args.PushMarkup(Loc.GetString("rune-examine", ("name", names), ("rituals", descriptions)));
    }

    private void OnIntearctHand(Entity<BloodCultRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.User))
            return;

        var invokersLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.InvokersLookupRange)
            .Where(q => HasComp<BloodCultistComponent>(q)).ToList();

        var invokers = new List<EntityUid>();
        foreach (var invoker in invokersLookup)
        {
            invokers.Add(invoker);
            if (HasComp<BloodCultistLeaderComponent>(invoker))
                invokers.Add(invoker); // Leaders counts as 2. Also gets twice as much i guess.
        }

        var invoked = false;
        CultRuneEvent? invokedEvent = null;
        var loc = string.Empty;
        foreach (var ev in ent.Comp.Events)
        {
            if (TryInvokeRune(ent, ev, invokers, out loc))
            {
                invoked = true;
                invokedEvent = ev;
                break; // no more
            }
        }

        if (!invoked)
        {
            if (!string.IsNullOrWhiteSpace(loc))
                _popup.PopupCursor(loc, args.User);
            return;
        }

        foreach (var invoker in invokers.Distinct().ToList())
            ProcessInvoker(invoker, invokedEvent);

        _colorFlash.RaiseEffect(Color.Red, [ent], Filter.Pvs(ent, entityManager: EntityManager));

        args.Handled = true;
    }

    private void ProcessInvoker(EntityUid invoker, CultRuneEvent? ev)
    {
        if (ev == null) return;

        var message = Loc.GetString(ev.InvokeLoc);
        _chat.TrySendInGameICMessage(invoker, message, InGameICChatType.Speak, true, hideLog: true);

        // todo make it get the hand used on the rune
        if (ev.Damage != null)
            _dmg.TryChangeDamage(invoker, ev.Damage, true, targetPart: TargetBodyPart.RightHand);
    }

    /// <summary>
    ///    Tries to invoke the rune.
    ///    The loc returned is what invokers will say on success.
    ///    On fail it's what the user will recieve as a popup in raw string form.
    /// </summary>
    public bool TryInvokeRune(Entity<BloodCultRuneComponent> ent, CultRuneEvent ev, List<EntityUid> invokers, out string loc)
    {
        loc = ev.InvokeLoc;

        if (invokers.Count < ev.RequiredInvokers)
        {
            loc = Loc.GetString("rune-invoke-fail-invokers", ("n", ev.RequiredInvokers - invokers.Count));
            return false;
        }

        var targetsLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.TargetsLookupRange)
            .Where(q => HasComp<HumanoidAppearanceComponent>(q))
            .Where(q => !HasComp<BloodCultistComponent>(q))
            .ToList();

        var @event = ev;
        @event!.Invokers = invokers;
        @event.Targets = new List<EntityUid>(targetsLookup);
        RaiseLocalEvent(ent, (object) @event);

        if (@event.Cancelled) return false;
        if (!string.IsNullOrWhiteSpace(ev.InvokeLoc)) loc = @event.InvokeLoc;

        return true;
    }
}
