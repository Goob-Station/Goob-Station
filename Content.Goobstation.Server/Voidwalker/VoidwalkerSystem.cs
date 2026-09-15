using Content.Goobstation.Shared.Voidwalker;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Voidwalker;
public sealed partial class VoidwalkerSystem : SharedVoidwalkerSystem
{
    [Dependency] private readonly DamageableSystem _damage = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly MetaDataSystem _meta = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private readonly ResPath _mapPath = new("Maps/_Goobstation/Nonstations/voidwalkervoid.yml");
    private static Entity<MapComponent>? _theVoid;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<VoidwalkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Check if spaced.
            if (curTime > comp.NextSpacedCheck)
            {
                UpdateSpacedStatus((uid, comp));
                comp.NextSpacedCheck = curTime + comp.SpacedCheckInterval;
            }

            // Healing tick
            if (curTime > comp.NextHealingTick
                && comp.IsInSpace)
            {
                if (comp.HealingWhenSpaced is { } healing)
                    _damage.TryChangeDamage(uid, healing);

                comp.NextHealingTick = curTime + comp.HealingTickInterval;
            }
        }
    }

    #region Event Handlers

    private void OnInit(Entity<VoidwalkerComponent> entity, ref MapInitEvent args)
    {
        // Load THE VOID map if not already loaded
        if (_theVoid == null
            && _mapLoader.TryLoadMap(_mapPath,
                out _theVoid,
                out _,
                new DeserializationOptions { InitializeMaps = true }))
            _map.SetPaused(_theVoid.Value.Comp.MapId, false);

        UpdateSpacedStatus(entity);
        _meta.AddFlag(entity, MetaDataFlags.ExtraTransformEvents);
    }

    private void OnCleanup(RoundRestartCleanupEvent args)
    {
        if (_theVoid is not null)
            QueueDel(_theVoid);

        _theVoid = null;
    }

    #endregion


}
