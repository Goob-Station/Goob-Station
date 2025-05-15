using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Item;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MantisBlades;

public sealed class SharedMantisBladeSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RightMantisBladeUserComponent, ComponentInit>(OnInitRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ComponentInit>(OnInitLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, ComponentShutdown>(OnShutdownRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ComponentShutdown>(OnShutdownLeft);

        SubscribeLocalEvent<MantisBladeArmComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInit(Entity<MantisBladeComponent> ent, ref ComponentInit args)
    {
        _item.SetHeldPrefix(ent, "popout");

        Timer.Spawn(ent.Comp.VisualDuration,
            () =>
        {
            if (!Deleted(ent))
                _item.SetHeldPrefix(ent, null);
        });
    }

    private void OnInitRight(Entity<RightMantisBladeUserComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionUid = _actions.AddAction(ent, ent.Comp.ActionProto);
    }

    private void OnInitLeft(Entity<LeftMantisBladeUserComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionUid = _actions.AddAction(ent, ent.Comp.ActionProto);
    }

    private void OnShutdownRight(Entity<RightMantisBladeUserComponent> ent, ref ComponentShutdown args)
    {
        Del(ent.Comp.BladeUid);
        _actions.RemoveAction(ent.Comp.ActionUid);
    }

    private void OnShutdownLeft(Entity<LeftMantisBladeUserComponent> ent, ref ComponentShutdown args)
    {
        Del(ent.Comp.BladeUid);
        _actions.RemoveAction(ent.Comp.ActionUid);
    }

    private void OnExamined(EntityUid uid, MantisBladeArmComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("mantis-blade-arm-examine"));
    }
}
