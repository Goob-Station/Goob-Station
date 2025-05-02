// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Spy.Roles;
using Content.Goobstation.Shared.Hologram;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components.Targets;
using Content.Server.PDA.Ringer;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Holopad;
using Content.Shared.Interaction;
using Content.Shared.Objectives;
using Content.Shared.PDA;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SpySystem
{
    private static readonly SoundSpecifier CodeGeneratedSound = new SoundPathSpecifier("/Audio/Items/Medical/healthscanner.ogg");
    private static readonly TimeSpan UplinkCreateDoAfterDuration = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    private void InitializeUplink()
    {
        SubscribeLocalEvent<PdaComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PdaComponent, UplinkCreateDoAfterEvent>(OnCreateUplink);
        SubscribeLocalEvent<SpyUplinkComponent, AfterInteractEvent>(OnInteractEvent);
        SubscribeLocalEvent<SpyUplinkComponent, SpyStealDoAfterEvent>(OnSpyAttemptSteal);
    }

    private void OnGetVerbs(Entity<PdaComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract)
            return;

        if (!_mind.TryGetMind(args.User, out var mindId, out _) // im sorry but this shit is disgusting in one if statement.
        || !_role.MindHasRole<SpyRoleComponent>(mindId)
        || HasComp<SpyUplinkComponent>(ent))
            return;

        var user = args.User;

        AlternativeVerb createUplinkVerb = new()
        {
            Act = () => {StartCreateDoAfter(ent, user);},
            Text = Loc.GetString("spy-create-uplink-verb"),
            Icon = new SpriteSpecifier.Rsi(new("Objects/Devices/communication.rsi"), "radio"),
            Priority = 1,
        };

        args.Verbs.Add(createUplinkVerb);
    }

    private void StartCreateDoAfter(Entity<PdaComponent> ent, EntityUid performer)
    {
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            performer,
            UplinkCreateDoAfterDuration,
            new UplinkCreateDoAfterEvent(),
            ent)
        {
            BreakOnMove = true,
            NeedHand = true,
            BlockDuplicate = true,
            BreakOnHandChange = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnCreateUplink(Entity<PdaComponent> ent, ref UplinkCreateDoAfterEvent args)
    {
        _uplink.AddUplink(args.User, 0, ent);

        var code = EnsureComp<RingerUplinkComponent>(ent).Code;
        EnsureComp<SpyUplinkComponent>(ent);

        var codeMessage = Loc.GetString($"traitor-role-uplink-code-short", ("code", string.Join("-", code).Replace("sharp", "#")));
        _antag.SendBriefing(args.User, codeMessage, Color.Crimson, CodeGeneratedSound);
    }

    private void OnInteractEvent(Entity<SpyUplinkComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return;

        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!TryComp<StealTargetComponent>(target, out var stealComp))
            return;

        // Find matching bounty
        var matchingBounty = dbEnt.Comp.Bounties.FirstOrDefault(b =>
            b.StealGroup == stealComp.StealGroup && !b.Claimed);

        if(!_protoMan.TryIndex<StealTargetGroupPrototype>(stealComp.StealGroup, out var prototype)) // store steal time here
            return;

        if (matchingBounty == null)
            return;

        var sound = _audio.PlayPvs(ent.Comp.StealSound, ent, AudioParams.Default.WithLoop(true));
        if (sound is null)
            return;

        EnsureComp<HologramComponent>(args.Target.Value);

        var doAfterArgs = new DoAfterArgs(_entityManager,
            args.User,
            ent.Comp.StealTime,
            new SpyStealDoAfterEvent(GetNetEntity(sound.Value.Entity), stealComp.StealGroup), // Now passing steal group instead of entity
            ent,
            target: target);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnSpyAttemptSteal(Entity<SpyUplinkComponent> ent, ref SpyStealDoAfterEvent args)
    {
        _audio.Stop(GetEntity(args.Sound));

        if (args.Handled
        || args.Cancelled
        || !TrySetBountyClaimed(ent, args.Args.User, args.StealGroup, out var bountyData))
            return;

        if (args.Args.Target != null)
            QueueDel(args.Args.Target.Value);

        var reward = Spawn(bountyData.RewardListing.ProductEntity, Transform(args.Args.User).Coordinates);
        _hands.PickupOrDrop(args.Args.User, reward);
        _audio.PlayLocal(ent.Comp.StealSuccessSound, ent, args.Args.User);
    }
}
