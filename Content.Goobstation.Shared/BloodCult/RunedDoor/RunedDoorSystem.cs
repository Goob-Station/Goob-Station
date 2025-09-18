using Content.Shared.Doors;
using Content.Shared.Prying.Components;
using Content.Shared.Repulsor;

namespace Content.Goobstation.Shared.BloodCult.RunedDoor;

public sealed class RunedDoorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RunedDoorComponent, BeforeDoorOpenedEvent>(OnBeforeDoorOpened);
        SubscribeLocalEvent<RunedDoorComponent, BeforeDoorClosedEvent>(OnBeforeDoorClosed);
        SubscribeLocalEvent<RunedDoorComponent, BeforePryEvent>(OnBeforePry);
        SubscribeLocalEvent<RunedDoorComponent, BeforeRepulseEvent>(BefoRepulse);
    }

    private void OnBeforeDoorOpened(Entity<RunedDoorComponent> door, ref BeforeDoorOpenedEvent args)
    {
        if (args.User is not { } user)
            return;

        if (!CanInteract(user))
            args.Cancel();
    }

    private void OnBeforeDoorClosed(Entity<RunedDoorComponent> door, ref BeforeDoorClosedEvent args)
    {
        if (args.User is not { } user)
            return;

        if (!CanInteract(user))
            args.Cancel();
    }

    private void OnBeforePry(Entity<RunedDoorComponent> door, ref BeforePryEvent args)
    {
        args.Cancelled = true;
    }

    private void BefoRepulse(Entity<RunedDoorComponent> door, ref BeforeRepulseEvent args)
    {
        if (HasComp<BloodCultist.BloodCultistComponent>(args.Target) || HasComp<Constructs.ConstructComponent>(args.Target))
            args.Cancel();
    }

    private bool CanInteract(EntityUid user)
    {
        return HasComp<BloodCultist.BloodCultistComponent>(user) || HasComp<Constructs.ConstructComponent>(user);
    }
}
