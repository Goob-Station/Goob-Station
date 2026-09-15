using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.GameTicking;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Voidwalker;

// TODO : Station compass pointer.

/// <summary>
/// Handles loading and de-loading the void.
/// </summary>
public sealed partial class VoidwalkerSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly AtmosphereSystem _atmos = null!;

    private readonly ResPath _mapPath = new("Maps/_Goobstation/Nonstations/voidwalkervoid.yml");
    private static Entity<MapComponent>? _theVoid;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);
    }

    private void OnInit(Entity<VoidwalkerComponent> entity, ref MapInitEvent args)
    {
        // Load THE VOID map if not already loaded
        if (_theVoid == null
            && _mapLoader.TryLoadMap(_mapPath,
                out _theVoid,
                out _,
                new DeserializationOptions { InitializeMaps = true }))
            _map.SetPaused(_theVoid.Value.Comp.MapId, false);

        _meta.AddFlag(entity, MetaDataFlags.ExtraTransformEvents);
    }

    private void OnCleanup(RoundRestartCleanupEvent args)
    {
        if (_theVoid is not null)
            QueueDel(_theVoid);

        _theVoid = null;
    }

}
