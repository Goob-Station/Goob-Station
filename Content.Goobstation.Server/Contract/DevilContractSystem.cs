using System.Text.RegularExpressions;
using Content.Goobstation.Common.Paper;
using Content.Goobstation.Server.Condemned;
using Content.Goobstation.Server.Devil;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Movement.Systems;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Contract;

public sealed partial class DevilContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly EntityManager _entityManager = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    private ISawmill _sawmill = null!;

    private TimeSpan _nextExecutionTime = TimeSpan.Zero;
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(2);

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("Contract");

        SubscribeLocalEvent<DevilContractComponent, BeingSignedAttemptEvent>(OnContractSignAttempt);
        SubscribeLocalEvent<DevilContractComponent, ExaminedEvent>(OnExamined);

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
        ["fear of space"] = -25,
        ["fear of fire"] = -20,
        ["fear of light"] = -15,
        ["fear of electricity"] = -15,
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

    public override void Update(float frametime)
    {
        base.Update(frametime);

        if (_timing.CurTime < _nextExecutionTime)
            return;

        TryUpdateContractWeight();

        _nextExecutionTime = _timing.CurTime + ExecutionInterval;
    }

    private void OnExamined(EntityUid uid, DevilContractComponent comp, ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient)
        {
            args.PushMarkup(Loc.GetString("devil-contract-examined", ("weight", comp.ContractWeight)));
        }
    }

    #region Signing Steps

    private void OnContractSignAttempt(EntityUid uid, DevilContractComponent comp, ref BeingSignedAttemptEvent args)
    {
        // Don't allow mortals to sign contracts for other people.
        // It won't work, but you still shouldn't be able to.
        if (comp.IsVictimSigned && args.Signer != comp.ContractOwner)
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
        _popupSystem.PopupEntity(Loc.GetString("contract-victim-signed"), args.User);

    }

    private void HandleDevilSign(EntityUid uid, DevilContractComponent comp, SignSuccessfulEvent args)
    {
        comp.IsDevilSigned = true;
        _popupSystem.PopupEntity(Loc.GetString("contract-devil-signed"), uid);

    }

    private void HandleBothPartiesSigned(EntityUid uid, DevilContractComponent comp)
    {
        // Common final activation logic
        TryContractEffects(uid, comp);

        // Make sure the weight is set properly.
        TryUpdateContractWeight();

        // Visual feedback
        // _appearance.SetData(uid, ContractVisuals.Active, true);
        // _audio.PlayGlobal("/Audio/Effects/contract_activate.ogg", Filter.Broadcast());

        // Maybe the contract burns after a couple of minutes?
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
            int newWeight = 0;

            foreach (Match match in matches)
            {
                if (!match.Success) continue;

                var clauseKey = match.Groups["clause"].Value.Trim().ToLower();
                if (_clauseWeights.TryGetValue(clauseKey, out var weight))
                {
                    newWeight += weight;
                }
                else
                {
                    _sawmill.Warning($"Unknown clause '{clauseKey}' in contract {uid}");
                }
            }

            // Update contract weight only if changed
            if (contract.ContractWeight != newWeight)
            {
                contract.ContractWeight = newWeight;
            }
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
            ApplyEffectToTarget(
                targetKey,       // string
                resolver(comp),  // EntityUid?
                comp,            // DevilContractComponent
                effect           // Action<...>
            );
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
                case "both":
                    if (Exists(comp.Signer))
                        effect(comp.Signer, comp);
                    if (Exists(comp.ContractOwner))
                        effect(comp.ContractOwner, comp);
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
