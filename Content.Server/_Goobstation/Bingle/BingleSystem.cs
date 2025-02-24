using Content.Shared.Weapons.Melee;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared._Goobstation.Bingle;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server._Goobstation.Bingle;

public sealed class BingleSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BingleComponent, AttackAttemptEvent>(OnAttackAttempt);
    }

    private void OnMapInit(EntityUid uid, BingleComponent component, MapInitEvent args)
    {
        var cords = Transform(uid).Coordinates;
        if (!cords.IsValid(EntityManager) || cords.Position == Vector2.Zero)
            return;
        if (MapId.Nullspace == Transform(uid).MapID)
            return;

        if (component.Prime)
            component.MyPit = Spawn("BinglePit", cords);
        else
        {
            var query = EntityQueryEnumerator<BinglePitComponent>();
            while (query.MoveNext(out var queryUid, out var _))
                if (cords == Transform(queryUid).Coordinates)
                    component.MyPit = queryUid;
        }
    }

    //ran by the pit to upgrade bingle damage
    public void UpgradeBingle(EntityUid uid, BingleComponent component)
    {
        if (component.Upgraded)
            return;
        if (!TryComp<MeleeWeaponComponent>(uid, out var weponComp))
            return;

        weponComp.Damage = component.UpgradeDamage;
        component.Upgraded = true;
        Dirty(uid, weponComp);

        _popup.PopupEntity(Loc.GetString("bingle-upgrade-success"), uid, uid);

        RaiseNetworkEvent(new BingleUpgradeEntityMessage(GetNetEntity(uid)));
    }

    private void OnAttackAttempt(EntityUid uid, BingleComponent component, AttackAttemptEvent args)
    {
        //Prevent Friendly Bingle fire
        if (HasComp<BinglePitComponent>(args.Target) || HasComp<BingleComponent>(args.Target))
            args.Cancel();
    }
}

