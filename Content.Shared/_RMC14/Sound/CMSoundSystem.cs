using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Sound;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._RMC14.Sound;

public sealed class CMSoundSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedEmitSoundSystem _emitSound = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RMCEmitSoundOnSpawnComponent, MapInitEvent>(OnEmitSpawnOnInit);

        SubscribeLocalEvent<SoundOnDeathComponent, MobStateChangedEvent>(OnDeathMobStateChanged);
        SubscribeLocalEvent<SoundOnDeathComponent, EntityTerminatingEvent>(OnDeathMobTerminating);

        SubscribeLocalEvent<SoundOnDeathSoundComponent, EntityTerminatingEvent>(OnDeathSoundTerminating);
    }

    private void OnEmitSpawnOnInit(Entity<RMCEmitSoundOnSpawnComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        if (ent.Comp.Sound == null)
            return;

        ent.Comp.Entity = _audio.PlayPvs(ent.Comp.Sound, ent.Owner)?.Entity;

        var coordinates = _transform.GetMoverCoordinates(ent);
        if (TerminatingOrDeleted(coordinates.EntityId))
            return;

        if (ent.Comp.Entity == null)
            return;

        _transform.SetCoordinates(ent.Comp.Entity.Value, coordinates);
        QueueDel(ent.Owner);
    }
    private void OnDeathMobStateChanged(Entity<SoundOnDeathComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!_net.IsServer)
            return;

        ent.Comp.Entity = _audio.PlayPvs(ent.Comp.Sound, ent)?.Entity;
        Dirty(ent);
    }

    private void OnDeathMobTerminating(Entity<SoundOnDeathComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.Entity == null ||
            TerminatingOrDeleted(ent.Comp.Entity))
        {
            return;
        }

        var coordinates = _transform.GetMoverCoordinates(ent);
        if (TerminatingOrDeleted(coordinates.EntityId))
            return;

        _transform.SetCoordinates(ent.Comp.Entity.Value, coordinates);
        ent.Comp.Entity = null;
    }

    private void OnDeathSoundTerminating(Entity<SoundOnDeathSoundComponent> ent, ref EntityTerminatingEvent args)
    {
        var parent = ent.Comp.Parent;
        ent.Comp.Parent = null;

        if (!TryComp(parent, out SoundOnDeathComponent? death))
            return;

        death.Entity = null;
        Dirty(parent.Value, death);
    }
}
