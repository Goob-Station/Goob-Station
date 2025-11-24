using Content.Shared.Doors;
using Content.Shared.Doors.Components;

namespace Content.Trauma.Shared.AudioMuffle;

public abstract class SharedAudioMuffleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoundBlockerComponent, DoorStateChangedEvent>(OnDoorStateChanged);
    }

    private void OnDoorStateChanged(Entity<SoundBlockerComponent> ent, ref DoorStateChangedEvent args)
    {
        switch (args.State)
        {
            case DoorState.Closed:
                ent.Comp.Active = true;
                break;
            case DoorState.Open:
                ent.Comp.Active = false;
                break;
            default:
                return;
        }

        DirtyField(ent.AsNullable(), nameof(SoundBlockerComponent.Active));
    }
}
