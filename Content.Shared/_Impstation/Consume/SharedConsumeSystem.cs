using Content.Shared._Impstation.Kodepiia.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Consume;

public abstract partial class SharedConsumeSystem : EntitySystem
{

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.ConsumeActionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<Components.ConsumeActionComponent, ComponentShutdown>(OnShutdown);
    }

    public sealed partial class ConsumeEvent : EntityTargetActionEvent;
    [Serializable, NetSerializable]
    public sealed partial class ConsumeDoAfterEvent : SimpleDoAfterEvent;

    public void OnShutdown(Entity<Components.ConsumeActionComponent> ent, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.ConsumeAction);
    }

    public void OnStartup(Entity<Components.ConsumeActionComponent> ent, ref ComponentStartup args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ConsumeAction, ent.Comp.ConsumeActionId);
    }
}
