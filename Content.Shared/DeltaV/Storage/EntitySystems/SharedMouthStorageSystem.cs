using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.DeltaV.Storage.Components;
using Content.Shared.Standing;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Shared.DeltaV.Storage.EntitySystems;

public abstract class SharedMouthStorageSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MouthStorageComponent, MapInitEvent>(OnMouthStorageInit);
        SubscribeLocalEvent<MouthStorageComponent, DownedEvent>(DropAllContents);
        SubscribeLocalEvent<MouthStorageComponent, DisarmedEvent>(DropAllContents);
        SubscribeLocalEvent<MouthStorageComponent, DamageChangedEvent>(OnDamageModified);
    }

    protected bool IsMouthBlocked(MouthStorageComponent component)
    {
        if (!TryComp<StorageComponent>(component.MouthId, out var storage))
            return false;

        return storage.Container.ContainedEntities.Count > 0;
    }

    private void OnMouthStorageInit(EntityUid uid, MouthStorageComponent component, MapInitEvent args)
    {
        if (string.IsNullOrWhiteSpace(component.MouthProto))
            return;

        component.Mouth = _containerSystem.EnsureContainer<Container>(uid, MouthStorageComponent.MouthContainerId);
        component.Mouth.ShowContents = false;
        component.Mouth.OccludesLight = false;

        var mouth = Spawn(component.MouthProto, new EntityCoordinates(uid, 0, 0));
        _containerSystem.Insert(mouth, component.Mouth);
        component.MouthId = mouth;

        if (!string.IsNullOrWhiteSpace(component.OpenStorageAction) && component.Action == null)
        {
            _actionsSystem.AddAction(uid, ref component.Action, component.OpenStorageAction, mouth);
        }
    }

    private void DropAllContents(EntityUid uid, MouthStorageComponent component, EntityEventArgs args)
    {
        if (component.MouthId == null)
            return;

        RaiseLocalEvent(component.MouthId.Value, new DumpContentsEvent(uid, uid));
    }

    private void OnDamageModified(EntityUid uid, MouthStorageComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null || !args.DamageIncreased)
            return;

        if (args.DamageDelta.GetTotal() < component.SpitDamageThreshold)
            return;

        DropAllContents(uid, component, args);
    }
}