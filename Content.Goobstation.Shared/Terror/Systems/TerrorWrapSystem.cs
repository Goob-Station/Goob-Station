using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Wraps someone up into a cocoon, permanently buffing both the spider that wrapped the person, and the hive as a whole.
/// </summary>
public sealed class TerrorWrapSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly SharedGameTicker _gameTicker = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorWrapComponent, TerrorWrapEvent>(OnTryWrap);

        SubscribeLocalEvent<TerrorWrapComponent, TerrorWrapDoAfterEvent>(OnWrapDoAfter);

    }

    private void OnTryWrap(Entity<TerrorWrapComponent> ent, ref TerrorWrapEvent args)
    {

        var target = args.Target;
        var uid = ent.Owner;
        //_popup.PopupPredicted(Loc.GetString("terror-wrap", ("user", ent.Owner), ("target", args.Target)), ent.Owner, args.Target, PopupType.LargeCaution);

        // TO DO: Change this FTL
        if (!_mobState.IsDead(target))
        {
            _popup.PopupClient(Loc.GetString("terror-wrap-fail"), uid, uid);
            return;
        }

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.DoAfter,
            new TerrorWrapDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnDamage = false,
            BreakOnMove = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        args.Handled = true;
    }

    private void OnWrapDoAfter(Entity<TerrorWrapComponent> ent, ref TerrorWrapDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
        {
            return;
        }

        var cocoon = EntityManager.SpawnEntity(ent.Comp.CocoonProto, Transform(target).Coordinates);

        _storage.Insert(target, cocoon);

        if (HasComp<TerrorSpiderComponent>(ent.Owner))
        {
            // Tell other systems a corpse was wrapped.
            var ev = new TerrorWrappedCorpseEvent(ent.Owner);
            RaiseLocalEvent(ent.Owner, ev);
        }

        // Used to increase the odds of better eggs being laid by the Queen.
        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent, GameRuleComponent>();

        while (rules.MoveNext(out var uid, out var rule, out _))
        {
            rule.TotalWrapped++;
            Dirty(uid, rule);
        }
        _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent.Owner)} cocooned {ToPrettyString(target)} as a Terror Spider.");
        args.Handled = true;
    }
}
