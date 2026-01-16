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
using System.Text;

namespace Content.Goobstation.Server.Cult.Runes;

public sealed partial class BloodCultRuneSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;

    public const float InvokersLookupRange = 1.5f;
    public const float TargetsLookupRange = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuneComponent, InteractHandEvent>(OnIntearctHand);
        SubscribeLocalEvent<BloodCultRuneComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<BloodCultRuneComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<BloodCultRuneComponent> ent, ref ExaminedEvent args)
    {
        var names = string.Empty;
        var descriptions = string.Empty;
        var trueCount = ent.Comp.Events.Count - 1;
        for (int i = 0; i < ent.Comp.Events.Count; i++)
        {
            var ev = ent.Comp.Events[i];
            var name = Loc.GetString(ev.InspectNameLoc);
            var desc = Loc.GetString(ev.InspectDescLoc);
            var color = ev.PulseColor.ToHex();

            // "name1 / name2 / name3"
            names += $"{((i > 0 && i < trueCount) ? " / " : "")}[color={color}]{name}[/color]";

            // "- description 1"
            // "\n- description 2"
            descriptions += $"\n- [color={color}]{desc}[/color]";
        }

        args.PushMarkup(Loc.GetString("rune-examine", ("name", names), ("rituals", descriptions)));
    }

    private void OnIntearctHand(Entity<BloodCultRuneComponent> ent, ref InteractHandEvent args)
    {
        var invokersLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, InvokersLookupRange)
            .Where(q => HasComp<BloodCultistComponent>(q)).ToList();

        var invoked = false;
        var loc = string.Empty;
        foreach (var ev in ent.Comp.Events)
        {
            if (!TryInvokeRune(ent, ev, invokersLookup, out loc))
                continue;
            invoked = true;
        }

        if (!invoked)
        {
            if (!string.IsNullOrWhiteSpace(loc))
                _popup.PopupCursor(loc, args.User);
            return;
        }

        foreach (var invoker in invokersLookup)
        {
            var message = Loc.GetString(loc);
            _chat.TrySendInGameICMessage(invoker, message, InGameICChatType.Speak, true, hideLog: true);
        }

        _colorFlash.RaiseEffect(Color.Red, [ent], Filter.Pvs(ent, entityManager: EntityManager));

        args.Handled = true;
    }

    private void OnInteractUsing(Entity<BloodCultRuneComponent> ent, ref InteractUsingEvent args)
    {
        if (HasComp<BloodCultRuneScribeComponent>(args.Used))
        {
            DestroyRune(ent);
            args.Handled = true;
            return;
        }
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

        var targetsLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, TargetsLookupRange)
            .Where(q => !HasComp<BloodCultistComponent>(q))
            .ToList();

        ev.Invokers = invokers;
        ev.Targets = targetsLookup;
        RaiseLocalEvent(ent, ev);

        if (ev.Cancelled) return false;
        if (!string.IsNullOrWhiteSpace(ev.InvokeLoc)) loc = ev.InvokeLoc;

        return true;
    }

    public void DestroyRune(Entity<BloodCultRuneComponent> ent)
    {
        QueueDel(ent);
        // todo effects and/or doafter
    }
}
