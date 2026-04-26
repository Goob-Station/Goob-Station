using Content.Shared._DV.Carrying;
using Content.Shared._Goobstation.Wizard.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Damage;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.SSDIndicator;
using Content.Shared.Storage.Components;
using Content.Shared.Strip.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

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
        SubscribeLocalEvent<ObraDinnHologramComponent, InsertIntoEntityStorageAttemptEvent>(OnStorage);
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

        var listenerr = EnsureComp<ActiveListenerComponent>(ent.Owner);// needed for ListenEvent
        listenerr.Range=ent.Comp.MinDistance;


        // comps we dont want the hologram to have
        RemCompDeferred<PullableComponent>(ent);
        RemCompDeferred<WoundableComponent>(ent);
        RemCompDeferred<ActorComponent>(ent);
        RemCompDeferred<MindContainerComponent>(ent);
        RemCompDeferred<FixturesComponent>(ent);
        RemCompDeferred<PhysicsComponent>(ent);
        RemCompDeferred<StrippableComponent>(ent);
        RemCompDeferred<SSDIndicatorComponent>(ent);
        RemCompDeferred<GhostRoleMobSpawnerComponent>(ent);
        RemCompDeferred<DamageableComponent>(ent);
        RemCompDeferred<MobMoverComponent>(ent);
        RemCompDeferred<CarriableComponent>(ent);
        RemCompDeferred<HasJobIconsComponent>(ent);
        RemCompDeferred<MobStateComponent>(ent);
    }

    private void OnShutdown(Entity<ObraDinnHologramComponent> ent, ref ComponentShutdown arg)
    {
        var effect =SpawnAtPosition(ent.Comp.SpawnEffect,Transform(ent).Coordinates);
        _audio.PlayPvs(ent.Comp.Sound, effect );
    }

    private void OnStorage(Entity<ObraDinnHologramComponent> ent, ref InsertIntoEntityStorageAttemptEvent arg)
    {
        arg.Cancelled = true;
    }
}
