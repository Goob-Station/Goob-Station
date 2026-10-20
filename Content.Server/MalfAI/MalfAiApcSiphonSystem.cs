// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Store.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.MalfAI;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Server.Power.Components;
using Content.Shared.Alert;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Localization;

namespace Content.Server.MalfAI;

/// <summary>
/// Handles the Malf AI's APC siphoning.
/// Pretty much their only method of getting CPU.
/// </summary>
public sealed class MalfAiApcSiphonSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly Content.Server.Power.EntitySystems.ApcSystem _apcSystem = default!;
    [Dependency] private readonly Robust.Shared.Timing.IGameTiming _timing = default!;

    private const string CpuCurrency = "CPU";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ApcComponent, Content.Shared.Verbs.GetVerbsEvent<Content.Shared.Verbs.AlternativeVerb>>(OnGetAltVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MalfAiApcSiphonedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.SiphonEndTime)
            {
                RemComp<MalfAiApcSiphonedComponent>(uid);
            }
        }
    }

    private void OnGetAltVerbs(EntityUid uid, ApcComponent apc, ref Content.Shared.Verbs.GetVerbsEvent<Content.Shared.Verbs.AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;
        
        if (!HasComp<MalfAiMarkerComponent>(args.User))
            return;

        // Ensure we haven't already siphoned it
        if (HasComp<MalfAiApcSiphonedComponent>(uid))
            return;

        var user = args.User;
        var verb = new Content.Shared.Verbs.AlternativeVerb()
        {
            Text = Loc.GetString("malf-ai-hack-apc-verb", ("target", uid)), // Will fallback if missing
            Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new Robust.Shared.Utility.ResPath("/Textures/Interface/VerbIcons/zap.svg.192dpi.png")),
            Priority = 10,
            Act = () =>
            {
                var siphonEvent = new ApcStartSiphonEvent(user);
                OnApcStartSiphon(uid, apc, ref siphonEvent);
            }
        };
        args.Verbs.Add(verb);
    }



    public void OnApcStartSiphon(EntityUid uid, ApcComponent apc, ref ApcStartSiphonEvent args)
    {
        if (HasComp<MalfAiApcSiphonedComponent>(uid))
            return;

        // Hack it: Ensure it's emagged
        var emagged = EnsureComp<Content.Shared.Emag.Components.EmaggedComponent>(uid);
        emagged.EmagType |= Content.Shared.Emag.Systems.EmagType.Interaction;
        Dirty(uid, emagged);

        var emagEvent = new Content.Shared.Emag.Systems.GotEmaggedEvent(args.SiphonedBy, Content.Shared.Emag.Systems.EmagType.Interaction);
        RaiseLocalEvent(uid, ref emagEvent);
        
        // Force visual state update so the APC turns blue immediately
        _apcSystem.UpdateApcState(uid, apc);

        var siphonedComp = EnsureComp<MalfAiApcSiphonedComponent>(uid);
        siphonedComp.SiphonEndTime = _timing.CurTime + TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.MalfAiCpuSiphonDuration));

        if (!TryComp<Content.Shared.Store.Components.StoreComponent>(args.SiphonedBy, out var store))
            return;

        var cpuAmount = _cfg.GetCVar(CCVars.MalfAiCpuSiphonAmount);
        var siphonAmount = FixedPoint2.New(cpuAmount);

        // Grant CPU to the AI
        var dict = new System.Collections.Generic.Dictionary<string, FixedPoint2> { { CpuCurrency, siphonAmount } };
        _store.TryAddCurrency(dict, args.SiphonedBy, store);

        // Log the APC siphoning for admin records
        _adminLogger.Add(LogType.Action, LogImpact.High, $"Malf AI {ToPrettyString(args.SiphonedBy)} siphoned APC {ToPrettyString(uid)} for {cpuAmount} CPU");
    }
}
