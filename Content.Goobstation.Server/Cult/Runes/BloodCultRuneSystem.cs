using Content.Goobstation.Shared.Cult;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.Chat;
using Content.Shared.Interaction;
using System.Linq;

namespace Content.Goobstation.Server.Cult.Runes;
public sealed partial class BloodCultRuneSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public const float InvokersLookupRange = 1.5f;
    public const float TargetsLookupRange = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultRuneComponent, InteractHandEvent>(OnIntearctHand);
        SubscribeLocalEvent<BloodCultRuneComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnIntearctHand(Entity<BloodCultRuneComponent> ent, ref InteractHandEvent args)
    {
        var invokersLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, InvokersLookupRange)
            .Where(q => HasComp<BloodCultistComponent>(q)).ToList();

        if (!TryInvokeRune(ent, invokersLookup, out var loc))
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
    }

    private void OnInteractUsing(Entity<BloodCultRuneComponent> ent, ref InteractUsingEvent args)
    {
        if (HasComp<BloodCultRuneScribeComponent>(args.Used))
        {
            DestroyRune(ent);
            args.Handled = true;
            return;
        }

        // mango
    }

    /// <summary>
    ///    Tries to invoke the rune.
    ///    The loc returned is what invokers will say on success.
    ///    On fail it's what the user will recieve as a popup in raw string form.
    /// </summary>
    public bool TryInvokeRune(Entity<BloodCultRuneComponent> ent, List<EntityUid> invokers, out string loc)
    {
        loc = ent.Comp.InvokeLoc;

        if (invokers.Count < ent.Comp.RequiredInvokers)
        {
            loc = Loc.GetString("rune-invoke-fail-invokers", ("n", ent.Comp.RequiredInvokers - invokers.Count));
            return false;
        }

        var targetsLookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, TargetsLookupRange).ToList();

        var ev = ent.Comp.Event;
        ev.Targets = targetsLookup;
        RaiseLocalEvent(ent, ev);

        if (ev.Cancelled) return false;
        if (!string.IsNullOrWhiteSpace(ev.InvokeLoc)) loc = ev.InvokeLoc;

        return true;
    }

    public void DestroyRune(Entity<BloodCultRuneComponent> ent)
    {

    }
}
