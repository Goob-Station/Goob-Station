// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 RatherUncreative <RatherUncreativeName@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
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

using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Targeting; // Shitmed Change
using Content.Shared._Shitmed.Damage;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Body.Organ;

namespace Content.Shared._Lavaland.Body;

public sealed class CursedHeartSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedHeartComponent, PumpHeartActionEvent>(OnPump);
        SubscribeLocalEvent<CursedHeartComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CursedHeartComponent, TryRemoveOrganEvent>(OnTryRemoveOrgan);
    }

    private void OnStartup(Entity<CursedHeartComponent> ent, ref ComponentStartup args)
    {
        _actions.AddAction(ent.Owner, ent.Comp.PumpAction);
        _audio.PlayGlobal(ent.Comp.Heartbeat, ent.Owner);
        ent.Comp.LastPump = _timing.CurTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CursedHeartComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var comp, out var state))
        {
            if (state.CurrentState is MobState.Critical or MobState.Dead)
                continue;

            if (_timing.CurTime < comp.LastPump + TimeSpan.FromSeconds(comp.MaxDelay))
                continue;

            var ent = (uid, comp);
            Damage(ent);
            comp.LastPump = _timing.CurTime;
        }
    }

    private void Damage(Entity<CursedHeartComponent> ent)
    {
        _bloodstream.TryModifyBloodLevel(ent.Owner, ent.Comp.BloodHarmMissedPump);
        _damage.TryChangeDamage(ent.Owner, ent.Comp.PumpHarm, true, false);
        _popup.PopupEntity(Loc.GetString("popup-cursed-heart-damage"), ent.Owner, ent.Owner, PopupType.MediumCaution);
    }

    private void OnPump(Entity<CursedHeartComponent> ent, ref PumpHeartActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        _audio.PlayGlobal(ent.Comp.Heartbeat, ent.Owner);
        _damage.TryChangeDamage(ent.Owner, ent.Comp.PumpHeal, true, false, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll); // Shitmed Change
        _bloodstream.TryModifyBloodLevel(ent.Owner, ent.Comp.BloodHealPerPump);
        ent.Comp.LastPump = _timing.CurTime;
    }

    private void OnTryRemoveOrgan(Entity<CursedHeartComponent> ent, ref TryRemoveOrganEvent args)
    {
        args.Cancelled = true;
    }
}
