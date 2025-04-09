// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Mobs;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Popups;
using Content.Shared.Weapons.Marker;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Weapons.Block;

public abstract partial class SharedBlockChargeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlockChargeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BlockChargeComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<BlockChargeUserComponent, BeforeDamageChangedEvent>(OnMeleeHit);
        SubscribeLocalEvent<BlockChargeComponent, ApplyMarkerBonusEvent>(OnMarkerBonus);
        SubscribeLocalEvent<BlockChargeComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<BlockChargeComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnMapInit(EntityUid uid, BlockChargeComponent component, MapInitEvent args)
    {
        component.NextCharge = _timing.CurTime + TimeSpan.FromSeconds(component.RechargeTime);
        Dirty(uid, component);
    }

    private void OnExamine(EntityUid uid, BlockChargeComponent component, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(component.HasCharge ? "block-charge-status-charged" : "block-charge-status-recharging"));
    }


    private void OnMarkerBonus(EntityUid uid, BlockChargeComponent component, ref ApplyMarkerBonusEvent args)
    {
        component.NextCharge -= TimeSpan.FromSeconds(component.MarkerReductionTime);
        Dirty(uid, component);
    }

    private void OnMeleeHit(EntityUid uid, BlockChargeUserComponent component, ref BeforeDamageChangedEvent args)
    {
        if (!TryComp<BlockChargeComponent>(component.BlockingWeapon, out var blockComp)
            || !HasComp<FaunaComponent>(args.Origin)
            || !blockComp.HasCharge
            || !args.CanEvade)
            return;

        _popup.PopupPredicted(Loc.GetString("block-attack-notice", ("user", uid), ("blocked", args.Origin)), uid, null);
        blockComp.HasCharge = false;
        blockComp.NextCharge = _timing.CurTime + TimeSpan.FromSeconds(blockComp.RechargeTime);
        Dirty(component.BlockingWeapon, blockComp);
        args.Cancelled = true;
    }

    private void OnEquipped(EntityUid uid, BlockChargeComponent component, GotEquippedHandEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var comp = EnsureComp<BlockChargeUserComponent>(args.User);
        comp.BlockingWeapon = uid;
        if (component.HasCharge)
            _popup.PopupClient(Loc.GetString("block-charge-startup", ("entity", uid)), args.User, args.User);

        Dirty(args.User, comp);
    }

    private void OnUnequipped(EntityUid uid, BlockChargeComponent component, GotUnequippedHandEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        RemCompDeferred<BlockChargeUserComponent>(args.User);
    }
}