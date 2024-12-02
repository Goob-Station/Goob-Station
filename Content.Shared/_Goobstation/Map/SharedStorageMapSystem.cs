using Content.Shared.GameTicking;

namespace Content.Shared._Goobstation.Map;

/// <summary>
///     System used to create a new paused/unpaused map to contain some entities on it.
///     Useful when it's hard/not possible to use containers instead.
/// </summary>
public abstract class SharedStorageMapSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public EntityUid? PausedMap { get; private set; }

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    /// <summary>
    ///     Cleanups maps after round restart
    /// </summary>
    private void OnRoundRestart(RoundRestartCleanupEvent _)
    {
        if (PausedMap == null || !Exists(PausedMap))
            return;

        EntityManager.DeleteEntity(PausedMap.Value);

        PausedMap = null;
    }

    public void SendToPausedStorageMap(EntityUid entity)
    {
        EnsurePausedMap();

        if (PausedMap != null)
            _transformSystem.SetParent(entity, PausedMap.Value);
    }

    protected void EnsurePausedMap()
    {
        if (PausedMap != null && Exists(PausedMap))
            return;

        var newMap = _mapSystem.CreateMap();
        _mapSystem.SetPaused(newMap, true);
        PausedMap = newMap;
    }

    /// <summary>
    ///     Checks if given entity in paused map right now
    /// </summary>
    public bool IsInPausedMap(EntityUid entity)
    {
        var xform = Transform(entity);

        return xform.MapUid != null && xform.MapUid == PausedMap;
    }
}
