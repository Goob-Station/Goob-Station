// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Goobstation.Shared.SpecialPassives.Fleshmend.Components;
using Content.Goobstation.Shared.SpecialPassives.SuperAdrenaline.Components;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stealth.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Goobstation.Shared.InternalResources.Components;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingSystem
{
    #region Dependencies

    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly WeldableSystem _weldable = default!; //for biodegrade unweld

    #endregion

    #region Constants

    private readonly ProtoId<DamageGroupPrototype> _absorbedDamageGroup = "Genetic";
    private readonly ProtoId<CollectiveMindPrototype> _hivemindProto = "Lingmind";
    private readonly EntProtoId _actionLayEgg = "ActionLayEgg";
    private readonly List<ProtoId<ReagentPrototype>> _biomassAbsorbedChemicals =
        ["Nutriment", "Protein", "UncookedAnimalProteins", "Fat"];

    #endregion

    private void SubscribeAbilities()
    {
        SubscribeBasicAbilities();
        SubscribeCombatAbilities();

        SubscribeLocalEvent<ChangelingIdentityComponent, StingReagentEvent>(OnStingReagent);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingTransformEvent>(OnStingTransform);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingFakeArmbladeEvent>(OnStingFakeArmblade);
        SubscribeLocalEvent<ChangelingIdentityComponent, StingLayEggsEvent>(OnLayEgg);

        SubscribeLocalEvent<ChangelingIdentityComponent, ActionAnatomicPanaceaEvent>(OnAnatomicPanacea);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionBiodegradeEvent>(OnBiodegrade);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionChameleonSkinEvent>(OnChameleonSkin);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionAdrenalineReservesEvent>(OnAdrenalineReserves);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionFleshmendEvent>(OnHealUltraSwag);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionLastResortEvent>(OnLastResort);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionLesserFormEvent>(OnLesserForm);
        SubscribeLocalEvent<ChangelingIdentityComponent, ActionHivemindAccessEvent>(OnHivemindAccess);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbBiomatterEvent>(OnAbsorbBiomatter);
        SubscribeLocalEvent<ChangelingIdentityComponent, AbsorbBiomatterDoAfterEvent>(OnAbsorbBiomatterDoAfter);
    }

    #region Utilities

    private void OnAnatomicPanacea(EntityUid uid, ChangelingIdentityComponent comp, ref ActionAnatomicPanaceaEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString("changeling-panacea"), uid, uid);

        var panacea = _compFactory.GetComponent<BoostedImmunityComponent>();
        panacea.AlertId = args.Alert;
        panacea.Duration = args.Duration;

        AddComp(uid, panacea, true);

        _alerts.ShowAlert(
            uid,
            args.Alert,
            cooldown: (_timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(args.Duration)),
            autoRemove: true);

        args.Handled = true;
    }

    private void OnBiodegrade(EntityUid uid, ChangelingIdentityComponent comp, ref ActionBiodegradeEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.Container.ContainedEntities.Count > 0)
        {
            var cuff = cuffs.LastAddedCuffs;
            _cuffs.Uncuff(uid, cuffs.LastAddedCuffs, cuff);
            QueueDel(cuff);
        }

        if (TryComp<EnsnareableComponent>(uid, out var ensnareable) &&
            ensnareable.Container.ContainedEntities.Count > 0)
        {
            var bola = ensnareable.Container.ContainedEntities[0];
            // Yes this is dumb, but trust me this is the best way to do this. Bola code is fucking awful.
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                uid,
                0,
                new EnsnareableDoAfterEvent(),
                uid,
                uid,
                bola));
            QueueDel(bola);
        }

        var parent = Transform(uid).ParentUid;

        if (TryComp<WeldableComponent>(parent, out var weldable))
        {
            if (weldable.IsWelded)
                _weldable.SetWeldedState(parent, false);
        }

        var soln = new Solution();
        soln.AddReagent("PolytrinicAcid", 10f);

        if (_pull.IsPulled(uid))
        {
            var puller = Comp<PullableComponent>(uid).Puller;
            if (puller != null)
            {
                _puddle.TrySplashSpillAt(puller.Value, Transform((EntityUid) puller).Coordinates, soln, out _);
                _stun.KnockdownOrStun(puller.Value, TimeSpan.FromSeconds(1.5), true);

                if (!TryComp(puller.Value, out StatusEffectsComponent? status))
                    return;

                _statusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(puller.Value,
                    "TemporaryBlindness",
                    TimeSpan.FromSeconds(2f),
                    true,
                    status);
                return;
            }
        }

        _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, soln, out _);

        args.Handled = true;
    }

    private void OnChameleonSkin(EntityUid uid, ChangelingIdentityComponent comp, ref ActionChameleonSkinEvent args)
    {
        if (args.Handled)
            return;

        if (!comp.ChameleonActive)
        {
            EnsureComp<StealthComponent>(uid);
            EnsureComp<StealthOnMoveComponent>(uid);
            _popup.PopupEntity(Loc.GetString("changeling-chameleon-start"), uid, uid);
            comp.ChameleonActive = true;
        }
        else
        {
            RemComp<StealthComponent>(uid);
            RemComp<StealthOnMoveComponent>(uid);
            _popup.PopupEntity(Loc.GetString("changeling-chameleon-end"), uid, uid);
            comp.ChameleonActive = false;
        }

        args.Handled = true;
    }

    private void OnAdrenalineReserves(EntityUid uid,
        ChangelingIdentityComponent comp,
        ref ActionAdrenalineReservesEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString("changeling-adrenaline"), uid, uid);

        var adrenaline = _compFactory.GetComponent<SuperAdrenalineComponent>();
        adrenaline.AlertId = args.Alert;
        adrenaline.Duration = args.Duration;
        adrenaline.PassiveDamage = args.PassiveDamage;

        AddComp(uid, adrenaline, true);

        _alerts.ShowAlert(
            uid,
            args.Alert,
            cooldown: (_timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(args.Duration)),
            autoRemove: true);

        args.Handled = true;
    }

    // john space made me do this
    private void OnHealUltraSwag(EntityUid uid, ChangelingIdentityComponent comp, ref ActionFleshmendEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString("changeling-fleshmend"), uid, uid);

        ApplyFleshmend(uid, args);

        _alerts.ShowAlert(
            uid,
            args.Alert,
            cooldown: (_timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(args.Duration)),
            autoRemove: true);

        args.Handled = true;
    }

    private void ApplyFleshmend(EntityUid uid, ActionFleshmendEvent args)
    {
        var fleshmend = _compFactory.GetComponent<FleshmendComponent>();
        fleshmend.AlertId = args.Alert;
        fleshmend.Duration = args.Duration;
        fleshmend.PassiveSound = args.PassiveSound;
        fleshmend.ResPath = args.ResPath;
        fleshmend.EffectState = args.EffectState;
        AddComp(uid, fleshmend, true);
    }

    private void OnLastResort(EntityUid uid, ChangelingIdentityComponent comp, ref ActionLastResortEvent args)
    {
        if (args.Handled)
            return;

        comp.IsInLastResort = true;

        var newUid = TransformEntity(
            uid,
            protoId: "MobHeadcrab",
            comp: comp,
            dropInventory: true,
            transferDamage: false);

        if (newUid == null)
        {
            comp.IsInLastResort = false;
            UpdateChemicals((uid, comp), Comp<InternalResourcesActionComponent>(args.Action).UseAmount);
            return;
        }

        _explosionSystem.QueueExplosion(
            newUid.Value,
            typeId: "Default",
            totalIntensity: 1,
            slope: 4,
            maxTileIntensity: 2);

        _actions.AddAction(newUid.Value, _actionLayEgg);

        PlayMeatySound(newUid.Value);

        args.Handled = true;
    }

    private void OnLesserForm(EntityUid uid, ChangelingIdentityComponent comp, ref ActionLesserFormEvent args)
    {
        if (args.Handled)
            return;

        comp.IsInLesserForm = true;
        var newUid = TransformEntity(uid, protoId: "MobMonkey", comp: comp);
        if (newUid is null)
        {
            comp.IsInLesserForm = false;
            UpdateChemicals((uid, comp), Comp<InternalResourcesActionComponent>(args.Action).UseAmount);
            return;
        }

        EnsureComp<AbsorbableComponent>(newUid
            .Value); // allow other changelings to absorb them (monkeys dont have this by default)
        PlayMeatySound(newUid.Value);

        args.Handled = true;
    }

    private void OnHivemindAccess(EntityUid uid, ChangelingIdentityComponent comp, ref ActionHivemindAccessEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<HivemindComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("changeling-passive-active"), uid, uid);
            return;
        }

        EnsureComp<HivemindComponent>(uid);
        var mind = EnsureComp<CollectiveMindComponent>(uid);
        mind.Channels.Add(_hivemindProto);
        mind.CanUseInCrit = true;

        _popup.PopupEntity(Loc.GetString("changeling-hivemind-start"), uid, uid);

        args.Handled = true;
    }

    #endregion
}
