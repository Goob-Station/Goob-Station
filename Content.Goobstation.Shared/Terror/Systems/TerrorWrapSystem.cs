using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Wraps a target into a cocoon, permanently buffing the wrapping spider's regen and the
/// hive's egg-laying odds.
/// </summary>
public sealed class TerrorWrapSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

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

        if (TryComp(uid, out TerrorSpiderComponent? spider) && _proto.TryIndex(spider.SpiderType, out var proto) && !proto.CanWrap)
        {
            _popup.PopupClient(Loc.GetString("terror-wrap-cannot"), uid, uid);
            return;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("terror-wrap-fail"), uid, uid); // TO DO: Unique pop-up
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupClient(Loc.GetString("terror-wrap-fail"), uid, uid);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.DoAfter, new TerrorWrapDoAfterEvent(), ent.Owner, args.Target)
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
            return;

        args.Handled = true;

        if (_netManager.IsClient)
            return;

        var cocoon = Spawn(ent.Comp.CocoonProto, Transform(target).Coordinates);

        if (!_storage.Insert(target, cocoon))
        {
            QueueDel(cocoon);
            _popup.PopupEntity(Loc.GetString("terror-wrap-insert-fail"), ent.Owner, ent.Owner);
            _admin.Add(LogType.Action, LogImpact.Medium, $"Failed to insert {ToPrettyString(target)} into cocoon spawned by {ToPrettyString(ent.Owner)}");
            return;
        }

        if (HasComp<TerrorSpiderComponent>(ent.Owner))
        {
            var ev = new TerrorWrappedCorpseEvent(ent.Owner);
            RaiseLocalEvent(ent.Owner, ref ev);
        }

        _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent.Owner)} cocooned {ToPrettyString(target)} as a Terror Spider.");
    }
}
