using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.ObraDinn;

/// <summary>
/// This handles temporary holograms from the obra dinn clock
/// </summary>
public sealed class ObraDinnHologramSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ObraDinnHologramComponent, ListenEvent>(Chat);
        SubscribeLocalEvent<ObraDinnHologramComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ObraDinnHologramComponent, ComponentShutdown>(OnShutdown);
    }

    private void Chat(Entity<ObraDinnHologramComponent> ent, ref ListenEvent arg)
    {
        if (!arg.Message.ToLower().Equals(ent.Comp.RealName.ToLower()))
            return;

        if (!Transform(ent.Owner).Coordinates.TryDistance(EntityManager,Transform(arg.Source).Coordinates, out var dist) || dist > ent.Comp.MinDistance )
            return;

        _metaData.SetEntityName(ent.Owner, ent.Comp.RealName);

        SpawnAtPosition(ent.Comp.SpawnEffect,Transform(ent).Coordinates);
        _audio.PlayPvs(ent.Comp.Sound, ent );
    }

    private void OnStartup(Entity<ObraDinnHologramComponent> ent, ref ComponentStartup arg)
    {
        SpawnAtPosition(ent.Comp.SpawnEffect,Transform(ent).Coordinates);
        _audio.PlayPvs(ent.Comp.Sound, ent );

        var lister= EnsureComp<ActiveListenerComponent>(ent.Owner);// needed for ListenEvent
        lister.Range=ent.Comp.MinDistance;
    }

    private void OnShutdown(Entity<ObraDinnHologramComponent> ent, ref ComponentShutdown arg)
    {
        var effect =SpawnAtPosition(ent.Comp.SpawnEffect,Transform(ent).Coordinates);
        _audio.PlayPvs(ent.Comp.Sound, effect );
    }
}
