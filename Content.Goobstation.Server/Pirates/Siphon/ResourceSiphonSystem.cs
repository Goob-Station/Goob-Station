// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Pirates;
using Content.Goobstation.Server.Pirates.GameTicking.Rules;
using Content.Goobstation.Server.Pirates.Objectives;
using Content.Server.Cargo.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Pirates.Siphon;

public sealed  class ResourceSiphonSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationAnchorSystem _anchor = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceSiphonComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ResourceSiphonComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<ResourceSiphonComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ResourceSiphonComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ResourceSiphonComponent, DestructionEventArgs>(OnDestruction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<ResourceSiphonComponent>();
        while (eqe.MoveNext(out var uid, out var siphon))
        {
            if (siphon.NextUpdateTime >_timing.CurTime)
                continue;

            siphon.NextUpdateTime += siphon.NextUpdateInterval;

            siphon.ActivationPhase -= 1;
            if (siphon.ActivationPhase < 0)
                siphon.ActivationPhase = 0; // reset

            Tick((uid, siphon));
        }
    }

    private void Tick(Entity<ResourceSiphonComponent> ent)
    {
        if (ent.Comp.Active)
            ActiveTick(ent);

        SyncWithGamerule(ent);
    }
    private void ActiveTick(Entity<ResourceSiphonComponent> ent)
    {
        if (!GetBank(ent, out var nbank))
            return;

        var bank = nbank!.Value;

        var funds = Siphon(ent, bank.Comp.Accounts, ent.Comp.DrainRate, ent.Comp.DrainPercent);

        UpdateCredits(ent, funds);
    }

    /// <summary>
    /// removes funds from the station eveanly distributad based on available funds in that department
    /// </summary>
    /// <param name="ent"></param> ent of the siphoning machine
    /// <param name="accounts"></param> accouunts of the station
    /// <param name="siphon"></param> ammount to siphon must be more than 0
    /// <returns>returns the ammount removed</returns>
    private float Siphon(Entity<ResourceSiphonComponent> ent, Dictionary<ProtoId<CargoAccountPrototype>, int> accounts, float siphon = 1f, float drainPercent = .1f)
    {
        var total = 0f;

        foreach (var (key, ammount) in accounts)
            if (ammount > 0)
                total += ammount; //we only care about positive accounts. red accounts get excluded

        siphon = Math.Max(total * drainPercent, siphon); // either drain a % of the total or take the set value if its bigger.
        siphon = (float) Math.Round(siphon); // round

        if (total < siphon || total == 0) // total is lower then we wanted so we take what we can.
        {
            foreach (var (key, ammount) in accounts)
                if (ammount > 0)
                    accounts[key] = 0;

            DeactivateSiphon(ent, "empty"); // stop the siphon because  all accounts are empty.
            return total;
        }

        foreach (var (key, ammount) in accounts)
            if (ammount > 0)
                accounts[key] -= (int) ((ammount/total) * siphon); //remove value from each account

        return siphon; // return what we came for
    }

    #region Event Handlers
    private void OnInit(Entity<ResourceSiphonComponent> ent, ref ComponentInit args)
    {
        if (!TryBindRule(ent))
            return;
    }

    private void OnInteract(Entity<ResourceSiphonComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Active)
            return;

        // no station = bad
        if (!GetBank(ent, out var bank))
        {
            var loc = Loc.GetString("pirate-siphon-nosignal");
            _chat.TrySendInGameICMessage(ent, loc, InGameICChatType.Speak, false);
            return;
        }

        // very far away from station = bad
        var dist = Vector2.Distance(_xform.GetWorldPosition(bank!.Value), _xform.GetWorldPosition(ent));
        if (dist > ent.Comp.MaxSignalRange)
        {
            var loc = Loc.GetString("pirate-siphon-weaksignal");
            _chat.TrySendInGameICMessage(ent, loc, InGameICChatType.Speak, false);
            return;
        }

        ent.Comp.ActivationPhase += 1;
        if (ent.Comp.ActivationPhase < 3)
        {
            var loc = Loc.GetString($"pirate-siphon-activate-{ent.Comp.ActivationPhase}");
            _chat.TrySendInGameICMessage(ent, loc, InGameICChatType.Speak, false);
        }
        else
            ActivateSiphon(ent);
    }

    private void OnInteractUsing(Entity<ResourceSiphonComponent> ent, ref InteractUsingEvent args)
    {
        if (HasComp<CashComponent>(args.Used))
        {
            var price = _pricing.GetPrice(args.Used);
            if (price == 0)
                return;

            UpdateCredits(ent, (float) price);
            QueueDel(args.Used);
            args.Handled = true;
        }

        // add more stuff here if needed
    }

    private void OnExamine(Entity<ResourceSiphonComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<ActivePirateRuleComponent>(ent.Comp.BoundGamerule, out var prule))
            return;

        args.PushMarkup(Loc.GetString("pirate-siphon-examine", ("num", prule.Credits ), ("max_num", ent.Comp.CreditsThreshold)));
    }

    private void OnDestruction(Entity<ResourceSiphonComponent> ent, ref DestructionEventArgs args)
    {
        DeactivateSiphon(ent, "broken");

        var speso = Spawn("SpaceCash", Transform(ent).Coordinates);
        if (TryComp<StackComponent>(speso, out var stack))
            stack.Count = (int) ent.Comp.Credits;
    }
    #endregion

    private void ActivateSiphon(Entity<ResourceSiphonComponent> ent)
    {
        ent.Comp.Active = true;

        if (TryComp<StationAnchorComponent>(ent, out var anchor))
            _anchor.SetStatus((ent, anchor), true);

        var coords = _xform.GetWorldPosition(Transform(ent));
        _chat.TrySendInGameICMessage(ent, Loc.GetString("data-siphon-activated"), InGameICChatType.Speak, false);

        var anloc = Loc.GetString("data-siphon-activated-announcement", ("pos", $"X: {coords.X}; Y: {coords.Y}"));
        _chat.DispatchGlobalAnnouncement(anloc, "Priority", colorOverride: Color.Red);
    }

    private void DeactivateSiphon(Entity<ResourceSiphonComponent> ent, string reason = "none")
    {
        if (!ent.Comp.Active)
            return;

        ent.Comp.Active = false;
        if (TryComp<StationAnchorComponent>(ent, out var anchor))
            _anchor.SetStatus((ent, anchor), false);

        _chat.TrySendInGameICMessage(ent, Loc.GetString($"data-siphon-deactivated-{reason}"), InGameICChatType.Speak, false);

        _chat.DispatchGlobalAnnouncement(Loc.GetString("pirate-siphon-deactivated-announcement"), "Priority", colorOverride: Color.Green);
    }

    public bool TryBindRule(Entity<ResourceSiphonComponent> ent)
    {
        var eqe = EntityQueryEnumerator<ActivePirateRuleComponent>();
        while (eqe.MoveNext(out var ruid, out var rule))
        {
            if (rule.BoundSiphon == null)
            {
                rule.BoundSiphon = ent;
                ent.Comp.BoundGamerule = ruid;
                return true;
            }
        }
        return false;
    }
    public EntityUid? GetRule(Entity<ResourceSiphonComponent> ent)
    {
        if (ent.Comp.BoundGamerule == null)
            TryBindRule(ent);

        return ent.Comp.BoundGamerule;
    }

    public bool SyncWithGamerule(Entity<ResourceSiphonComponent> ent)
    {
        if (GetRule(ent) == null
        || !TryComp<ActivePirateRuleComponent>(ent.Comp.BoundGamerule, out var prule))
            return false;

        prule.Credits += ent.Comp.Credits;
        ent.Comp.Credits = 0;

        foreach (var pirate in prule.Pirates)
            if (_mind.TryGetObjectiveComp<ObjectivePlunderComponent>(pirate, out var objective))
                objective.Plundered = prule.Credits;

        return true;
    }

    public void UpdateCredits(Entity<ResourceSiphonComponent> ent, float amount)
    {
        if (!TryComp<ActivePirateRuleComponent>(ent.Comp.BoundGamerule, out var prule))
        {
            ent.Comp.Credits += amount;
            return;
        }

        prule.Credits += amount;
    }

    private bool GetBank(Entity<ResourceSiphonComponent> ent, out Entity<StationBankAccountComponent>? bank)
    {
        bank = null;
        var stationent = _station.GetStationInMap(Transform(ent).MapID);

        // no station
        if (stationent == null)
            return false;

        // no bank account
        if (!TryComp<StationBankAccountComponent>(stationent, out var bankaccount))
            return false;

        bank = (stationent.Value, bankaccount);
        return true;
    }
}
