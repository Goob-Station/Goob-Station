using Content.Goobstation.Server.Cult.GameTicking;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Goobstation.Shared.Cult.Runes;
using Content.Goobstation.Shared.UserInterface;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.Flash;
using Content.Shared.Chat;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Goobstation.Server.Cult.Systems;

public sealed partial class BloodCultRuneSystem : EntitySystem
{
    [Dependency] private readonly BloodCultRuleSystem _cultRule = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly FlashSystem _flash = default!;

    private void SubscribeRunes()
    {
        SubscribeLocalEvent<BloodCultRuneComponent, CultRuneOfferEvent>(OnCultRuneOfferEvent);
        SubscribeLocalEvent<BloodCultRuneComponent, CultRuneSacrificeEvent>(OnCultRuneSacrificeEvent);
        SubscribeLocalEvent<BloodCultRuneComponent, CultRuneEmpowerEvent>(OnCultRuneEmpowerEvent);
        SubscribeLocalEvent<BloodCultRuneComponent, CultRuneTeleportEvent>(OnCultRuneTeleportEvent);
    }

    private void OnCultRuneOfferEvent(Entity<BloodCultRuneComponent> ent, ref CultRuneOfferEvent args)
    {
        if (args.Handled
        || !_cultRule.TryGetRule(out var rule)
        || args.Targets == null || args.Targets.Count == 0)
        {
            args.Cancelled = true;
            return;
        }
        var target = args.Targets.First();

        if (TryComp<ActorComponent>(target, out var actor)
        && TryComp<AntagSelectionComponent>(rule, out var asc))
            // you make the AntagSelectionComponent too sweet for me to not be using this
            _antag.MakeAntag((rule.Value, asc), actor.PlayerSession, asc.Definitions.First());

#if DEBUG
        else _cultRule.MakeCultist(target, rule!.Value);
#endif

        _flash.Flash(target, null, null, TimeSpan.FromSeconds(3), 0, displayPopup: false, stunDuration: TimeSpan.FromSeconds(1));

#if !DEBUG
        // 1 in 1000 chance of the offeree saying the funny
        // guaranteed on debug!:tm:
        if (_rand.Prob(.001f))
#endif
        _chat.TrySendInGameICMessage(target, Loc.GetString("rune-invoke-offering-funny"), InGameICChatType.Speak, false);
        args.Handled = true;
    }

    private void OnCultRuneSacrificeEvent(Entity<BloodCultRuneComponent> ent, ref CultRuneSacrificeEvent args)
    {
        if (args.Handled
        || !_cultRule.TryGetRule(out var rule)
        || args.Targets == null || args.Targets.Count == 0)
        {
            args.Cancelled = true;
            return;
        }
        var target = args.Targets.First();

        if (!_mobState.IsDead(target))
        {
            args.Cancelled = true;
            return;
        }

        // todo soul stones
    }

    private void OnCultRuneEmpowerEvent(Entity<BloodCultRuneComponent> ent, ref CultRuneEmpowerEvent args)
    {
        if (args.Handled
        || args.Invokers == null || args.Invokers.Count == 0)
        {
            args.Cancelled = true;
            return;
        }
        var invoker = args.Invokers.First();

        if (!TryComp<BloodMagicProviderComponent>(invoker, out var magic))
        {
            args.Cancelled = true;
            return;
        }

        _ui.TryOpenUi(ent.Owner, EntityRadialMenuKey.Key, invoker);
    }

    private void OnCultRuneTeleportEvent(Entity<BloodCultRuneComponent> ent, ref CultRuneTeleportEvent args)
    {
        // todo name selector ui
    }
}
