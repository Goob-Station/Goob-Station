using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Gravity;
using Content.Shared.Mech;
using Content.Shared.Mech.Components;
using Content.Shared.Movement.Components;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class MechThrusterModuleSystem : EntitySystem
{
    [Dependency] private SharedGravitySystem _gravity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechThrusterModuleComponent, MechEquipmentInsertedEvent>(OnInserted);
        SubscribeLocalEvent<MechThrusterModuleComponent, MechEquipmentRemovedEvent>(OnRemoved);

        SubscribeLocalEvent<MechComponent, IsWeightlessEvent>(OnIsWeightless);
    }

    private void OnInserted(Entity<MechThrusterModuleComponent> module, ref MechEquipmentInsertedEvent args)
    {
        module.Comp.Installed = true;
        if (!HasComp<MovementAlwaysTouchingComponent>(args.Mech))
        {
            AddComp<MovementAlwaysTouchingComponent>(args.Mech);
            module.Comp.AddedAlwaysTouching = true;
        }

        Dirty(module);
        _gravity.RefreshWeightless(args.Mech);
    }

    private void OnRemoved(Entity<MechThrusterModuleComponent> module, ref MechEquipmentRemovedEvent args)
    {
        module.Comp.Installed = false;
        if (module.Comp.AddedAlwaysTouching)
        {
            RemComp<MovementAlwaysTouchingComponent>(args.Mech);
            module.Comp.AddedAlwaysTouching = false;
        }

        Dirty(module);
        _gravity.RefreshWeightless(args.Mech);
    }

    private void OnIsWeightless(Entity<MechComponent> mech, ref IsWeightlessEvent args)
    {
        if (args.Handled || mech.Comp.EquipmentContainer is not { } container || Transform(mech).GridUid == null)
            return;

        foreach (var equipment in container.ContainedEntities)
        {
            if (!TryComp<MechThrusterModuleComponent>(equipment, out var thruster) || !thruster.Installed)
                continue;

            args.IsWeightless = false;
            args.Handled = true;
            return;
        }
    }
}
