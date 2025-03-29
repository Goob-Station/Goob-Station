using Content.Shared.Examine;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.Religion.Nullrod;

public sealed class NullrodTransformSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AltarSourceComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, AltarSourceComponent component, InteractUsingEvent args)
    {


        if (args.Handled
            || !_netManager.IsServer
            || HasComp<StorageComponent>(args.Target) // If it's a storage component like a bag, we ignore usage so it can be stored.
            || !_tagSystem.HasTag(args.Used, "Nullrod") // Checks used entity for the tag we need.
            )
            return;

        Spawn(component.EffectProto, Transform(uid).Coordinates);
        _audio.PlayPvs(component.SoundPath, uid, AudioParams.Default.WithVolume(-4f));
        var nullrodUid = Spawn(component.RodProto, args.ClickLocation.SnapToGrid(EntityManager));
        var xform = Transform(nullrodUid); // Spawns entity assigned in RodProto.

        QueueDel(args.Used); // Deletes the previous entity.
        args.Handled = true;
    }
}
