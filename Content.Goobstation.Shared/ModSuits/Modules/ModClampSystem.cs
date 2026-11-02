using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModClampSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [SubscribeLocalEvent]
    private void OnInit(Entity<ModClampComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
    }

    [SubscribeLocalEvent]
    private void OnExamined(Entity<ModClampComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("modsuit-clamp-examine",
            ("count", ent.Comp.Container.ContainedEntities.Count),
            ("max", ent.Comp.MaxContents)));
    }

    [SubscribeLocalEvent]
    private void OnAfterInteract(Entity<ModClampComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (args.Target is { } target && CanGrab(ent, target, args.User))
        {
            args.Handled = true;
            if (ent.Comp.Container.ContainedEntities.Count >= ent.Comp.MaxContents)
            {
                _popup.PopupClient(Loc.GetString("modsuit-clamp-full"), args.User, args.User);
                return;
            }

            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.GrabDelay, new ModClampGrabDoAfterEvent(), ent, target, ent)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
            });
            _audio.PlayPredicted(ent.Comp.Sound, ent, args.User);
            return;
        }

        if (ent.Comp.Container.ContainedEntities.Count == 0)
            return;

        args.Handled = true;
        var ev = new ModClampDropDoAfterEvent(GetNetCoordinates(args.ClickLocation));
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.DropDelay, ev, ent, used: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
        });
    }

    [SubscribeLocalEvent]
    private void OnGrab(Entity<ModClampComponent> ent, ref ModClampGrabDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        args.Handled = true;
        if (!CanGrab(ent, target, args.User) || ent.Comp.Container.ContainedEntities.Count >= ent.Comp.MaxContents)
            return;

        _container.Insert(target, ent.Comp.Container);
        _popup.PopupClient(Loc.GetString("modsuit-clamp-grab", ("item", target)), args.User, args.User);
    }

    [SubscribeLocalEvent]
    private void OnDrop(Entity<ModClampComponent> ent, ref ModClampDropDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Container.ContainedEntities.Count == 0)
            return;

        args.Handled = true;
        var item = ent.Comp.Container.ContainedEntities[^1];
        _container.Remove(item, ent.Comp.Container, destination: GetCoordinates(args.Coordinates));
        _audio.PlayPredicted(ent.Comp.Sound, ent, args.User);
    }

    private bool CanGrab(Entity<ModClampComponent> ent, EntityUid target, EntityUid user)
    {
        if (target == user || target == ent.Owner || ent.Comp.Container.Contains(target))
            return false;

        var xform = Transform(target);
        if (xform.Anchored || TryComp<PhysicsComponent>(target, out var physics) && physics.BodyType == BodyType.Static)
            return false;

        return _whitelist.IsWhitelistPass(ent.Comp.Whitelist, target);
    }
}

[Serializable, NetSerializable]
public sealed partial class ModClampGrabDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class ModClampDropDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public NetCoordinates Coordinates { get; private set; }

    private ModClampDropDoAfterEvent() { }

    public ModClampDropDoAfterEvent(NetCoordinates coordinates)
    {
        Coordinates = coordinates;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
