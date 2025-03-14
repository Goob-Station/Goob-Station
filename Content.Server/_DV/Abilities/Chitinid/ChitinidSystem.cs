using Content.Server.Nutrition.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Abilities.Chitinid;

public sealed partial class ChitinidSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ChitinidComponent, ChitziteActionEvent>(OnChitzite);
        SubscribeLocalEvent<ChitinidComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ChitinidComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var chitinid, out var damageable))
        {
            if (_timing.CurTime < chitinid.NextUpdate)
                continue;

            chitinid.NextUpdate += chitinid.UpdateInterval;

            if (chitinid.AmountAbsorbed >= chitinid.MaximumAbsorbed || _mobState.IsDead(uid))
                continue;

            if (_damageable.TryChangeDamage(uid, chitinid.Healing, damageable: damageable) is {} delta)
            {
                chitinid.AmountAbsorbed += -delta.GetTotal().Float();
                if (chitinid.ChitziteAction != null && chitinid.AmountAbsorbed >= chitinid.MaximumAbsorbed)
                {
                    _actions.SetCharges(chitinid.ChitziteAction, 1); // You get the charge back and that's it. Tough.
                    _actions.SetEnabled(chitinid.ChitziteAction, true);
                }
            }
        }

        var entQuery = EntityQueryEnumerator<CoughingUpChitziteComponent, ChitinidComponent>();
        while (entQuery.MoveNext(out var ent, out var chitzite, out var chitinid))
        {
            if (_timing.CurTime < chitzite.NextCough)
                continue;

            Spawn(chitinid.ChitzitePrototype, Transform(ent).Coordinates);
            chitinid.AmountAbsorbed = 0f;
            RemCompDeferred(ent, chitzite);
        }
    }

    private void OnMapInit(Entity<ChitinidComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.ChitziteAction != null)
            return;

        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;

        _actions.AddAction(ent, ref ent.Comp.ChitziteAction, ent.Comp.ChitziteActionId);
    }

    private void OnChitzite(Entity<ChitinidComponent> ent, ref ChitziteActionEvent args)
    {
        if (_inventory.TryGetSlotEntity(ent, "mask", out var maskUid) &&
            TryComp<IngestionBlockerComponent>(maskUid, out var blocker) &&
            blocker.Enabled)
        {
            _popup.PopupEntity(Loc.GetString("chitzite-mask", ("mask", maskUid)), ent, ent);
            return;
        }

        _popup.PopupEntity(Loc.GetString("chitzite-cough", ("name", Identity.Entity(ent, EntityManager))), ent);
        _audio.PlayPvs("/Audio/Animals/cat_hiss.ogg", ent, AudioHelpers.WithVariation(0.15f));

        var chitzite = EnsureComp<CoughingUpChitziteComponent>(ent);
        chitzite.NextCough = _timing.CurTime + chitzite.CoughUpTime;
        args.Handled = true;
    }
}
