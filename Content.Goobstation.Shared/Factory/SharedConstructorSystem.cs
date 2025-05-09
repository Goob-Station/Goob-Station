using Content.Goobstation.Common.Construction;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

public abstract class SharedConstructorSystem : EntitySystem
{
    [Dependency] protected readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] protected readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructorComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ConstructorComponent, ConstructedEvent>(OnConstructed);
        Subs.BuiEvents<ConstructorComponent>(ConstructorUiKey.Key, subs =>
        {
            subs.Event<ConstructorSetProtoMessage>(OnSetProto);
        });
    }

    private void OnExamined(Entity<ConstructorComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var msg = ent.Comp.Construction is {} id
            ? Loc.GetString("constructor-examine", ("name", Proto.Index(id).Name))
            : Loc.GetString("constructor-examine-unset");
        args.PushMarkup(msg);
    }

    private void OnConstructed(Entity<ConstructorComponent> ent, ref ConstructedEvent args)
    {
        _transform.SetCoordinates(args.Entity, OutputPosition(ent));
    }

    private void OnSetProto(Entity<ConstructorComponent> ent, ref ConstructorSetProtoMessage args)
    {
        if (ent.Comp.Construction == args.Id || !Proto.HasIndex(args.Id))
            return;

        ent.Comp.Construction = args.Id;
        Dirty(ent);
        _adminLogger.Add(LogType.Construction, LogImpact.Low, $"{ToPrettyString(args.Actor):user} set {ToPrettyString(ent):target} construction to {args.Id}");
    }

    public EntityCoordinates OutputPosition(EntityUid uid)
    {
        var xform = Transform(uid);
        var offset = xform.LocalRotation.ToVec();
        return xform.Coordinates.Offset(offset);
    }
}
