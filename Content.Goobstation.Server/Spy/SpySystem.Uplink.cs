using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SpySystem
{
    private static readonly SoundSpecifier StealSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/wewewew.ogg");
    private static readonly TimeSpan StealTime = TimeSpan.FromSeconds(15);

    /// <inheritdoc/>
    private void InitializeUplink()
    {
        SubscribeLocalEvent<SpyUplinkComponent, AfterInteractEvent>(OnInteractEvent);
        SubscribeLocalEvent<SpyUplinkComponent, SpyStealDoAfterEvent>(OnSpyAttemptSteal);
    }

    private void OnSpyAttemptSteal(Entity<SpyUplinkComponent> ent, ref SpyStealDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is null)
            return;

        var target = args.Args.Target.Value;
        _audio.Stop(GetEntity(args.Sound));
        TrySetBountyClaimed(GetNetEntity(target));
        QueueDel(args.Args.Target);
    }

    private void OnInteractEvent(Entity<SpyUplinkComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return;

        if (args.Handled
            || !args.CanReach
            || args.Target is not { } target
            || dbEnt.Comp.Bounties.Any(bounty => bounty.TargetEntity != GetNetEntity(target)))
            return;

        var sound = _audio.PlayPvs(StealSound, ent, AudioParams.Default.WithLoop(true));

        if (sound is null)
            return;

        var doAfterArgs = new DoAfterArgs(_entityManager,
            args.User,
            StealTime,
            new SpyStealDoAfterEvent(GetNetEntity(sound.Value.Entity)),
            ent,
            target: args.Target);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
}
