using System.Numerics;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.SupermatterHalberd;

public sealed class SupermatterHalberdSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RaysSystem _rays = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SupermatterHalberdComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SupermatterHalberdComponent, SmHalberdExecuteDoAfterEvent>(OnExecute);
    }

    private void OnExecute(Entity<SupermatterHalberdComponent> ent, ref SmHalberdExecuteDoAfterEvent args)
    {
        if (args.Handled || args.Target == null)
            return;

        var (uid, comp) = ent;

        var ray = GetEntity(args.RayEffect);
        if (_net.IsServer && ray != null)
            QueueDel(ray);

        if (args.Cancelled)
        {
            if (HasComp<MobStateComponent>(args.Target.Value))
                _popup.PopupClient(Loc.GetString("supermatter-halberd-execution-cancel", ("used", uid)), args.Target.Value, args.User);
            return;
        }

        args.Handled = true;

        var targetIdentity = Identity.Entity(args.Target.Value, EntityManager);
        var userIdentity = Identity.Entity(args.User, EntityManager);

        _popup.PopupClient(Loc.GetString("supermatter-halberd-execution-end-user",
                ("target", targetIdentity),
                ("used", uid)),
            args.Target.Value,
            args.User);

        if (_net.IsClient)
            return;

        _admin.Add(HasComp<MobStateComponent>(args.Target.Value) ? LogType.Gib : LogType.InteractUsing,
            LogImpact.Extreme,
            $"{ToPrettyString(args.User):user} ashed {ToPrettyString(args.Target.Value):target} using {ToPrettyString(uid):used}");

        var xform = Transform(args.Target.Value);
        _popup.PopupCoordinates(Loc.GetString("supermatter-halberd-execution-end-other",
                ("target", targetIdentity),
                ("used", uid),
                ("user", userIdentity)),
            xform.Coordinates,
            Filter.PvsExcept(args.User),
            true,
            PopupType.LargeCaution);

        _audio.PlayPvs(comp.ExecuteSound, uid);
        var (pos, rot) = _transform.GetWorldPositionRotation(xform);
        var coords = new MapCoordinates(pos, xform.MapID);
        Del(args.Target);
        Spawn(comp.AshProto, coords, rotation: rot);
        Spawn(comp.ExecuteEffect, coords);
    }

    private void OnAfterInteract(Entity<SupermatterHalberdComponent> ent, ref AfterInteractEvent args)
    {
        var (uid, comp) = ent;

        if (args.Target == null)
            return;

        if (!_whitelist.IsValid(comp.ObliterateWhitelist, args.Target.Value) &&
            (!TryComp(args.Target.Value, out MobStateComponent? mobState) || mobState.CurrentState != MobState.Dead))
            return;

        var rayEffect = _rays.DoRays(_transform.GetMapCoordinates(args.Target.Value),
            Color.Yellow,
            Color.Orange,
            minMaxRadius: new Vector2(3f, 6f));

        var doArgs = new DoAfterArgs(EntityManager,
            args.User,
            comp.ExecuteDelay,
            new SmHalberdExecuteDoAfterEvent(GetNetEntity(rayEffect)),
            uid,
            args.Target,
            uid)
        {
            NeedHand = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = false,
            MultiplyDelay = false,
        };

        if (!_doAfter.TryStartDoAfter(doArgs))
        {
            if (_net.IsServer && rayEffect != null)
                QueueDel(rayEffect.Value);
            return;
        }

        var targetIdentity = Identity.Entity(args.Target.Value, EntityManager);
        var userIdentity = Identity.Entity(args.User, EntityManager);

        _popup.PopupClient(Loc.GetString("supermatter-halberd-execution-start-user",
                ("target", targetIdentity),
                ("used", args.Used)),
            args.User);
        _popup.PopupEntity(Loc.GetString("supermatter-halberd-execution-start-other",
                ("target", targetIdentity),
                ("used", args.Used),
                ("user", userIdentity)),
            args.User,
            Filter.PvsExcept(args.User),
            true,
            PopupType.LargeCaution);
    }
}

[Serializable, NetSerializable]
public sealed partial class SmHalberdExecuteDoAfterEvent(NetEntity? rayEffect) : DoAfterEvent
{
    public NetEntity? RayEffect = rayEffect;

    public SmHalberdExecuteDoAfterEvent() : this(null) { }

    public override DoAfterEvent Clone() => this;
}
