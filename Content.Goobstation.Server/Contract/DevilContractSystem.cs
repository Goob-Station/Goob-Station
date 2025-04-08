// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Goobstation.Common.Paper;
using Content.Goobstation.Server.Condemned;
using Content.Goobstation.Server.Devil;
using Content.Goobstation.Shared.Devil;
using Content.Server.Administration.Systems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Contract;

public sealed partial class DevilContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly EntityManager _entityManager = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly HungerSystem _hungerSystem = null!;

    private ISawmill _sawmill = null!;
    private readonly EntProtoId _fireEffectProto = "FireEffect";

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("Contract");

        SubscribeLocalEvent<DevilContractComponent, BeingSignedAttemptEvent>(OnContractSignAttempt);
        SubscribeLocalEvent<DevilContractComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilContractComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        SubscribeLocalEvent<DevilContractComponent, SignSuccessfulEvent>(OnSignStep);
    }

    #region Dictionaries

    private readonly Dictionary<string, Action<EntityUid, DevilContractComponent>> ContractClauses;

    private readonly Dictionary<string, Func<DevilContractComponent, EntityUid?>> _targetResolvers = new()
    {
        // The contractee is who is making the deal.
        ["contractee"] = comp => comp.Signer,
        // The contractor is the entity offering the deal.
        ["contractor"] = comp => comp.ContractOwner,
        // Both is the entity offering, and the entity making the deal.
        ["both"] = comp => null
    };

    // The lower the value, the more beneficial it is for the contractee.
    private readonly Dictionary<string, int> _clauseWeights = new()
    {
        ["mortality"] = -100,
        ["weakness"] = -35,
        ["death"] = -25,
        ["shadows"] = -25,
        ["fear of space"] = -25,
        ["fear of fire"] = -20,
        ["fear of light"] = -15,
        ["fear of electricity"] = -15,
        ["loneliness"] = -5,
        ["soul ownership"] = 25,
        ["strength"] = 25,
        ["coherence"] = 25,
        ["voice"] = 30,
        ["a hand"] = 30,
        ["a leg"] = 30,
        ["sanity"] = 35,
        ["legs"] = 40,
        ["an organ"] = 45,
        ["sight"] = 60,
        ["will to fight"] = 80,
        ["time"] = 150,

    };

    #endregion

    private static readonly Regex ClauseRegex = new(
        @"^\s*(?<target>Contractor|Contractee|Both)\s*:\s*(?<clause>.+?)\s*$", // Unholy.
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline
    );

    private void OnGetVerbs(EntityUid uid, DevilContractComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !TryComp<DevilComponent>(args.User, out var devilComp))
            return;

        if (!TryComp<DevilContractComponent>(uid, out var contractComp))
            return;

        AlternativeVerb burnVerb = new()
        {
            Act = () => TryBurnContract(uid, contractComp,  devilComp),
            Text = Loc.GetString("burn-contract-prompt"),
            Icon = new SpriteSpecifier.Rsi(new ("/Textures/Effects/fire.rsi"), "fire"),
        };

        args.Verbs.Add(burnVerb);
    }

    private void TryBurnContract(EntityUid contract, DevilContractComponent contractComponent, DevilComponent devilComp)
    {
        var coordinates = Transform(contract).Coordinates;

        if (contractComponent.ContractOwner == null)
            return;

        if (contractComponent is { IsDevilSigned: true, IsVictimSigned: true } or { IsDevilSigned: false, IsVictimSigned: false })
        {
            Spawn(_fireEffectProto, coordinates);
            _audio.PlayPvs(devilComp.FwooshPath, coordinates, new AudioParams(-2f, 1f, SharedAudioSystem.DefaultSoundRange, 1f, false, 0f));
            _popupSystem.PopupCoordinates(Loc.GetString("burn-contract-popup-success"), coordinates, PopupType.MediumCaution);
            QueueDel(contract);
        }
        else
        {
            _popupSystem.PopupCoordinates(Loc.GetString("burn-contract-popup-fail"), coordinates, (EntityUid)contractComponent.ContractOwner, PopupType.MediumCaution);
        }
    }

    private void OnExamined(EntityUid uid, DevilContractComponent comp, ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient)
        {
            TryUpdateContractWeight();
            args.PushMarkup(Loc.GetString("devil-contract-examined", ("weight", comp.ContractWeight)));
        }
    }

    #region Signing Steps

    private void OnContractSignAttempt(EntityUid uid, DevilContractComponent comp, ref BeingSignedAttemptEvent args)
    {
        // Make sure that weight is set properly!
        TryUpdateContractWeight();
        // Don't allow mortals to sign contracts for other people.
        // Also don't let silicons sell their souls, they don't have one.
        // It won't work, but you still shouldn't be able to.
        if (comp.IsVictimSigned && args.Signer != comp.ContractOwner && HasComp<SiliconComponent>(args.Signer))
        {
            _popupSystem.PopupEntity(Loc.GetString("devil-sign-invalid-user"), uid);
            return;
        }

        // Only handle unsigned contracts.
        if (comp.IsVictimSigned || comp.IsDevilSigned)
            return;

        // You can't sell your soul if you already sold it.
        if (HasComp<CondemnedComponent>(args.Signer))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("devil-contract-no-soul-sign-failed"),
                args.Signer,
                PopupType.MediumCaution
            );
            args.Cancelled = true;
        }

        // Check if the weight is too low
        if (comp.ContractWeight < 0)
        {
            var difference = Math.Abs(comp.ContractWeight);
            _popupSystem.PopupEntity(Loc.GetString("contract-uneven-odds", ("number", difference)),
                uid,
                PopupType.MediumCaution);
            args.Cancelled = true;
        }

        // Check if devil is trying to sign first
        if (args.Signer == comp.ContractOwner)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("devil-contract-early-sign-failed"),
                args.Signer,
                PopupType.MediumCaution
            );
            args.Cancelled = true;
        }
    }

    private void OnSignStep(EntityUid uid, DevilContractComponent comp, SignSuccessfulEvent args)
    {
        // Determine signing phase
        if (!comp.IsVictimSigned)
        {
            HandleVictimSign(uid, comp, args);
        }
        else if (!comp.IsDevilSigned)
        {
            HandleDevilSign(uid, comp, args);
        }

        // Final activation check
        if (comp.IsDevilSigned && comp.IsVictimSigned)
        {
            HandleBothPartiesSigned(uid, comp);
        }
    }

    private void HandleVictimSign(EntityUid uid, DevilContractComponent comp, SignSuccessfulEvent args)
    {
        // No funny business with a cybersun pen!
        if (TryComp<PaperComponent>(args.Paper, out var paper))
            paper.EditingDisabled = true;

        comp.Signer = args.User;
        comp.IsVictimSigned = true;
        _popupSystem.PopupEntity(Loc.GetString("contract-victim-signed"), args.Paper, args.User);

    }

    private void HandleDevilSign(EntityUid uid, DevilContractComponent comp, SignSuccessfulEvent args)
    {
        comp.IsDevilSigned = true;
        _popupSystem.PopupEntity(Loc.GetString("contract-devil-signed"), args.Paper, args.User);

    }

    private void HandleBothPartiesSigned(EntityUid uid, DevilContractComponent comp)
    {
        // Common final activation logic
        TryUpdateContractWeight();
        TryContractEffects(uid, comp);
    }

    #endregion

    #region Helper Events

    public bool TryTransferSouls(EntityUid devil, EntityUid contractee, int added)
    {
        if (HasComp<CondemnedComponent>(contractee))
            return false;

        var ev = new SoulAmountChangedEvent(devil, contractee, added);
        RaiseLocalEvent(devil, ref ev);

        var condemned = EnsureComp<CondemnedComponent>(contractee);
        condemned.SoulOwner = devil;
        condemned.CondemnOnDeath = true;

        return true;
    }

    private void TryUpdateContractWeight()
    {
        var query = EntityQueryEnumerator<DevilContractComponent>();
        while (query.MoveNext(out var uid, out var contract))
        {
            if (!TryComp<PaperComponent>(uid, out var paper))
                continue;

            var matches = ClauseRegex.Matches(paper.Content);
            var newWeight = 0;

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                var clauseKey = match.Groups["clause"].Value.Trim().ToLower();
                if (_clauseWeights.TryGetValue(clauseKey, out var weight))
                    newWeight += weight;
                else
                    _sawmill.Warning($"Unknown clause '{clauseKey}' in contract {uid}");
            }

            contract.ContractWeight = newWeight;
        }
    }

    private void TryContractEffects(EntityUid uid, DevilContractComponent comp)
    {
        if (!TryComp<PaperComponent>(uid, out var paper))
            return;

        var matches = ClauseRegex.Matches(paper.Content);

        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            var targetKey = match.Groups["target"].Value.Trim().ToLower();
            var clauseKey = match.Groups["clause"].Value.Trim().ToLower();

            if (!_targetResolvers.TryGetValue(targetKey, out var resolver))
            {
                _sawmill.Warning($"Invalid contract target: {targetKey}");
                continue;
            }

            if (!ContractClauses.TryGetValue(clauseKey, out var effect))
            {
                _sawmill.Warning($"Unknown contract clause: {clauseKey}");
                continue;
            }


            // Pass all four required parameters
            ApplyEffectToTarget(targetKey, resolver(comp), comp, effect);
        }
    }

    private void ApplyEffectToTarget(
        string targetKey,
        EntityUid? target,
        DevilContractComponent comp,
        Action<EntityUid, DevilContractComponent> effect)
    {
        try
        {
            switch (targetKey.ToLower())
            {
                // Scrapped idea, but it's funnier if I leave it tbh.
                case "both":
                    if (Exists(comp.Signer))
                        effect((EntityUid)comp.Signer, comp);
                    if (Exists(comp.ContractOwner))
                        effect((EntityUid)comp.ContractOwner, comp);
                    break;
                default:
                    if (target.HasValue && Exists(target.Value))
                        effect(target.Value, comp);
                    else
                        _sawmill.Warning($"Invalid target for {targetKey} clause");
                    break;
            }
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to apply {targetKey} clause: {ex}");
        }
    }

    #endregion
}
