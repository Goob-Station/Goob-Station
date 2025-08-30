using Content.Server._DV.CosmicCult.Components;
using Content.Server.Bible.Components;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicPurifiableSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CosmicPurifiableComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CosmicPurifiableComponent, EventPurgeRiftDoAfter>(OnPurgeDoAfter);
    }

    private void OnInteractUsing(Entity<CosmicPurifiableComponent> uid, ref InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<BibleComponent>(args.Used))
            return;

        _popup.PopupEntity(Loc.GetString("cosmiccult-rift-beginpurge"), args.User, args.User);
        var doargs = new DoAfterArgs(EntityManager,
            args.User,
            (HasComp<BibleUserComponent>(args.User) ? uid.Comp.CleanseTimeChaplain : uid.Comp.CleanseTime),
            new EventPurgeRiftDoAfter(),
            uid,
            uid)
        {
            DistanceThreshold = 1.5f, Hidden = false, BreakOnDamage = true, BreakOnDropItem = true,
            BreakOnMove = true, MovementThreshold = 2f,
        };
        _doAfter.TryStartDoAfter(doargs);
    }

    private void OnPurgeDoAfter(Entity<CosmicPurifiableComponent> uid, ref EventPurgeRiftDoAfter args)
    {
        if (args.Args.Target == null || args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        var tgtpos = Transform(uid).Coordinates;
        Spawn(uid.Comp.PurgeVFX, tgtpos);
        _audio.PlayPvs(uid.Comp.PurgeSound, args.User);
        _popup.PopupCoordinates(
            Loc.GetString("cosmiccult-rift-purge", ("NAME", Identity.Entity(args.Args.User, EntityManager))),
            Transform(args.Args.User).Coordinates,
            PopupType.Medium);
        QueueDel(uid);
    }
}
