using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Robust.Shared.Serialization;
using Content.Shared._Impstation.Kodepiia.Components;

namespace Content.Shared._Impstation.Kodepiia;

public abstract partial class SharedKodepiiaConsumeSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KodepiiaConsumeActionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<KodepiiaConsumeActionComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<KodepiiaConsumedComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<KodepiiaConsumedComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    // TODO: kinda weird but dunno a better way other than somehow making it directly heal when the genetic is healed
    private void OnMobStateChange(Entity<KodepiiaConsumedComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            RemComp<KodepiiaConsumedComponent>(ent);
    }

    private void OnExamine(Entity<KodepiiaConsumedComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString($"kodepiia-consumed-onexamine-{ent.Comp.Count}",
            ("target", Identity.Entity(ent, EntityManager))));
    }

    public void OnShutdown(Entity<KodepiiaConsumeActionComponent> ent, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.ConsumeAction);
    }

    public void OnStartup(Entity<KodepiiaConsumeActionComponent> ent, ref ComponentStartup args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ConsumeAction, ent.Comp.ConsumeActionId);
    }

    public sealed partial class KodepiiaConsumeEvent : EntityTargetActionEvent;

    [Serializable, NetSerializable]
    public sealed partial class KodepiiaConsumeDoAfterEvent : SimpleDoAfterEvent;
}