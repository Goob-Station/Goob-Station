using Content.Shared._CorvaxGoob.ReplaceOnInteractWith;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Server._CorvaxGoob.ReplaceOnInteractWith;

public sealed class ReplaceOnInteractWithSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplaceOnInteractWithComponent, AfterInteractEvent>(OnAfterInteractEvent);
        SubscribeLocalEvent<ReplaceOnInteractWithComponent, ReplaceOnInteractWithDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteractEvent(Entity<ReplaceOnInteractWithComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is null)
            return;

        if (_whitelist.IsWhitelistFail(entity.Comp.Whitelist, args.Target.Value))
            return;

        if (_whitelist.IsBlacklistPass(entity.Comp.Blacklsit, args.Target.Value))
            return;

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, entity.Comp.Delay, new ReplaceOnInteractWithDoAfterEvent(), entity, target: args.Target, used: entity));
    }

    private void OnDoAfter(Entity<ReplaceOnInteractWithComponent> entity, ref ReplaceOnInteractWithDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Args.Target is null)
            return;

        if (entity.Comp.ReplaceSound is not null)
            _audioSystem.PlayPvs(entity.Comp.ReplaceSound, args.User);

        Spawn(entity.Comp.Prototype, _xform.GetMoverCoordinates(args.Args.Target.Value));
        Del(args.Args.Target);
        Del(entity);
    }
}
