using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModEnergyShieldModuleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ModEnergyShieldModuleComponent>();
        while (query.MoveNext(out var uid, out var shield))
        {
            if (shield.Charges >= shield.MaxCharges || now < shield.NextCharge)
                continue;

            shield.Charges++;
            shield.NextCharge = now + shield.ChargeIncrement;
            Dirty(uid, shield);
        }
    }

    [SubscribeLocalEvent]
    private void OnDamageModify(Entity<ModEnergyShieldModuleComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.Charges <= 0
            || args.Origin is not { } origin
            || origin == args.Target
            || args.Damage.GetTotal() <= 0
            || !_modSuit.TryGetModuleSuit(ent, out var suit))
            return;

        args.Damage = new DamageSpecifier();
        ent.Comp.Charges--;
        ent.Comp.NextCharge = _timing.CurTime + ent.Comp.RechargeDelay;
        Dirty(ent);

        _modSuit.TryUseCharge(suit, ent.Comp.BlockPower);

        if (_net.IsClient)
            return;

        _audio.PlayPvs(ent.Comp.BlockSound, args.Target);
        _popup.PopupEntity(Loc.GetString("modsuit-energy-shield-block"), args.Target, PopupType.Medium);
    }

    [SubscribeLocalEvent]
    private void OnExamined(Entity<ModEnergyShieldModuleComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("modsuit-energy-shield-examine",
            ("charges", ent.Comp.Charges),
            ("max", ent.Comp.MaxCharges)));
    }
}
