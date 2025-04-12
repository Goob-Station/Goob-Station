// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Text.RegularExpressions;
using Content.Goobstation.Common.Paper;
using Content.Goobstation.Server.Devil.Condemned;
using Content.Goobstation.Shared.Devil;
using Content.Server.Body.Systems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Devil.Contract;

public sealed class DevilContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = null!;
    [Dependency] private readonly BodySystem _bodySystem = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    private ISawmill _sawmill = null!;
    private readonly EntProtoId _fireEffectProto = "FireEffect";

    public override void Initialize()
    {
        base.Initialize();
        InitializeRegex();

        _sawmill = Logger.GetSawmill("Contract");

        SubscribeLocalEvent<DevilContractComponent, BeingSignedAttemptEvent>(OnContractSignAttempt);
        SubscribeLocalEvent<DevilContractComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilContractComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        SubscribeLocalEvent<DevilContractComponent, SignSuccessfulEvent>(OnSignStep);
    }

    private readonly Dictionary<LocId, Func<DevilContractComponent, EntityUid?>> _targetResolvers = new()
    {
        // The contractee is who is making the deal.
        ["devil-contract-contractee"] = comp => comp.Signer,
        // The contractor is the entity offering the deal.
        ["devil-contract-contractor"] = comp => comp.ContractOwner,
    };

    private Regex _clauseRegex = null!;
    private readonly Dictionary<string, LocId> _localizedTargetToLocId = new (StringComparer.OrdinalIgnoreCase);

    private void InitializeRegex()
    {
        var escapedPatterns = new List<string>();
        foreach (var locId in _targetResolvers.Keys)
        {
            var localized = Loc.GetString(locId);
            escapedPatterns.Add(localized);
            _localizedTargetToLocId[localized] = locId;
        }

        var targetPattern = string.Join("|", escapedPatterns);
        _clauseRegex = new Regex($@"^\s*(?<target>{targetPattern})\s*:\s*(?<clause>.+?)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

    }

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
            TryUpdateContractWeight(uid, comp);
            args.PushMarkup(Loc.GetString("devil-contract-examined", ("weight", comp.ContractWeight)));
        }
    }

    #region Signing Steps

    private void OnContractSignAttempt(EntityUid uid, DevilContractComponent comp, ref BeingSignedAttemptEvent args)
    {
        // Make sure that weight is set properly!
        TryUpdateContractWeight(uid, comp);
        // Don't allow mortals to sign contracts for other people.
        if (comp.IsVictimSigned && args.Signer != comp.ContractOwner)
        {
            _popupSystem.PopupEntity(Loc.GetString("devil-sign-invalid-user"), uid);
            args.Cancelled = true;
            return;
        }

        // Only handle unsigned contracts.
        if (comp.IsVictimSigned || comp.IsDevilSigned)
            return;

        // You can't sell your soul if you already sold it. (also no robits)
        if (HasComp<CondemnedComponent>(args.Signer) || HasComp<SiliconComponent>(args.Signer))
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
            HandleVictimSign(uid, comp, args);
        else if (!comp.IsDevilSigned)
            HandleDevilSign(uid, comp, args);

        // Final activation check
        if (comp is { IsDevilSigned: true, IsVictimSigned: true })
            HandleBothPartiesSigned(uid, comp);
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
        TryUpdateContractWeight(uid, comp);
        TryContractEffects(uid, comp);
    }

    #endregion

    #region Helper Events

    public bool TryTransferSouls(EntityUid devil, EntityUid contractee, int added)
    {
        // Can't sell what doesn't exist.
        if (HasComp<CondemnedComponent>(contractee))
            return false;

        // Can't sell yer soul to yourself
        if (devil == contractee)
            return false;

        var ev = new SoulAmountChangedEvent(devil, contractee, added);
        RaiseLocalEvent(devil, ref ev);

        var condemned = EnsureComp<CondemnedComponent>(contractee);
        condemned.SoulOwner = devil;
        condemned.CondemnOnDeath = true;

        return true;
    }

    private void TryUpdateContractWeight(EntityUid uid, DevilContractComponent contract)
    {
        if (!TryComp<PaperComponent>(uid, out var paper))
            return;

        contract.CurrentClauses.Clear();
        var matches = _clauseRegex.Matches(paper.Content);
        var newWeight = 0;

        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            var clauseKey = match.Groups["clause"].Value.Trim().ToLowerInvariant().Replace(" ", "");

            if (_prototypeManager.TryIndex(clauseKey, out DevilClausePrototype? clauseProto))
            {
                newWeight += clauseProto.ClauseWeight;
                contract.CurrentClauses.Add(clauseProto);
            }
            else
                _sawmill.Warning($"Unknown clause '{clauseKey}' in contract {uid}");
        }

        contract.ContractWeight = newWeight;
    }

    private void TryContractEffects(EntityUid uid, DevilContractComponent comp)
    {
        if (!TryComp<PaperComponent>(uid, out var paper))
            return;

        var matches = _clauseRegex.Matches(paper.Content);

        foreach (Match match in matches)
        {
            if (!match.Success)
                continue;

            var targetKey = match.Groups["target"].Value.Trim().ToLowerInvariant().Replace(" ", "");
            var clauseKey = match.Groups["clause"].Value.Trim().ToLowerInvariant().Replace(" ", "");

            var locId = _targetResolvers.Keys.FirstOrDefault(id => Loc.GetString(id).Equals(targetKey, StringComparison.OrdinalIgnoreCase));
            var resolver = _targetResolvers[locId];

            if (resolver(comp) == null)
            {
                _sawmill.Warning($"Unknown resolver: {resolver(comp)}");
                continue;
            }

            if (!_prototypeManager.TryIndex(clauseKey, out DevilClausePrototype? clause))
            {
                _sawmill.Warning($"Unknown contract clause: {clauseKey}");
                continue;
            }

            var targetEntity = resolver(comp);
            if (targetEntity != null && !TerminatingOrDeleted(targetEntity.Value))
                ApplyEffectToTarget(targetEntity.Value, comp, clause);
            else
                _sawmill.Warning($"Invalid target entity from resolver for clause {clauseKey} in contract {uid}");
        }
    }

    private void ApplyEffectToTarget(EntityUid target, DevilContractComponent contract, DevilClausePrototype clause)
    {
        if (clause.AddedComponents != null)
        {
            foreach (var comp in clause.AddedComponents.Select(component => component.Value)) // im linqing it
                EntityManager.AddComponent(target, comp);
        }

        if (clause.RemovedComponents != null)
        {
            foreach (var component in clause.RemovedComponents.Select(component => component.Value.Component))
                EntityManager.RemoveComponent(target, component);
        }

        if (clause.DamageModifierSet != null)
            _damageable.SetDamageModifierSetId(target, clause.DamageModifierSet);

        if (clause.SpecialActions == null)
            return;

        foreach (var specialAction in clause.SpecialActions)
        {
            switch (specialAction)
            {
                case "SoulOwnership":
                    if (contract.ContractOwner != null)
                        TryTransferSouls(contract.ContractOwner.Value, target, 1);
                    break;

                case "RemoveHand":
                    TryComp<BodyComponent>(target, out var body);
                    var hands = _bodySystem.GetBodyChildrenOfType(target, BodyPartType.Hand, body).ToList();
                    if (hands.Count > 0)
                    {
                        var pick = _random.Pick(hands);
                        _transform.AttachToGridOrMap(pick.Id);
                        QueueDel(pick.Id);
                    }
                    break;

                case "RemoveLeg":
                    TryComp<BodyComponent>(target, out var bodyLeg);
                    var legs = _bodySystem.GetBodyChildrenOfType(target, BodyPartType.Leg, bodyLeg).ToList();
                    if (legs.Count > 0)
                    {
                        var pick = _random.Pick(legs);
                        _transform.AttachToGridOrMap(pick.Id);
                        QueueDel(pick.Id);
                    }
                    break;

                case "RemoveOrgan":
                    var organs = _bodySystem.GetBodyOrgans(target).ToList();
                    if (organs.Count > 0)
                    {
                        var pick = _random.Pick(organs);
                        _transform.AttachToGridOrMap(pick.Id);
                        QueueDel(pick.Id);
                    }
                    break;
            }
        }
    }

    #endregion
}
