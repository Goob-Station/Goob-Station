using Content.Goobstation.Common.Mech;
using Content.Goobstation.Shared.Audio;
using Content.Goobstation.Shared.Mech.Components;

namespace Content.Goobstation.Server.Mech.Systems;

public sealed class MechCabinAudioSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, MechInsertedEvent>(OnPilotInserted);
        SubscribeLocalEvent<AudioReverbComponent, MechEjectedEvent>(OnPilotEjected);
    }

    private void OnPilotInserted(Entity<MetaDataComponent> pilot, ref MechInsertedEvent args)
    {
        if (!TryComp<MechCabinAudioComponent>(args.mechUid, out var cabin))
            return;

        var reverb = EnsureComp<AudioReverbComponent>(pilot);
        reverb.Preset = cabin.Preset;
        Dirty(pilot, reverb);
    }

    private void OnPilotEjected(Entity<AudioReverbComponent> pilot, ref MechEjectedEvent args)
    {
        RemComp<AudioReverbComponent>(pilot);
    }
}
