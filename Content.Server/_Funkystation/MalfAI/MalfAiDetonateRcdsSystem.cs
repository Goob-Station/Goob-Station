using Content.Server.Explosion.EntitySystems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Content.Shared.Store.Components;
using Robust.Shared.Timing;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI Detonate RCDs action. Extracted from MalfAiShopSystem.
/// </summary>
public sealed class MalfAiDetonateRcdsSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosions = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private static readonly TimeSpan RcdDetonationDelay = TimeSpan.FromSeconds(5);
    private static readonly SoundSpecifier RcdBeepSound = new SoundPathSpecifier("/Audio/Effects/beep1.ogg");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreComponent, MalfAiDetonateRcdsActionEvent>(OnDetonateAllRcdsAction);
    }

    private void OnDetonateAllRcdsAction(EntityUid uid, StoreComponent comp, ref MalfAiDetonateRcdsActionEvent args)
    {
        var performer = args.Performer != default ? args.Performer : uid;
        ArmRcdsOnGrid(performer);
        args.Handled = true;
    }

    /// <summary>
    /// Arms all RCDs on the same grid as the given entity, warning holders, beeping each second, then detonating and deleting them.
    /// </summary>
    private void ArmRcdsOnGrid(EntityUid origin)
    {
        if (!Exists(origin))
            return;

        var perfXform = Transform(origin);
        var gridUid = perfXform.GridUid;
        if (gridUid == null)
            return;

        var query = EntityQueryEnumerator<RCDComponent, TransformComponent>();
        while (query.MoveNext(out var rcdUid, out _, out var xform))
        {
            if (xform.GridUid != gridUid)
                continue;

            // Skip detonation if RCD has a cyborg module component (engi borgs protection)
            if (HasComp<BorgModuleComponent>(rcdUid))
                continue;

            if (_containers.TryGetContainingContainer((rcdUid, xform, null), out var container))
            {
                var owner = container.Owner;
                if (_hands.IsHolding(owner, rcdUid, out _))
                {
                    var msg = Loc.GetString("detonate_rcd_warning");
                    _popup.PopupEntity(msg, owner, owner, PopupType.LargeCaution);
                }
            }

            var targetRcd = rcdUid;

            var totalSeconds = (int) Math.Floor(RcdDetonationDelay.TotalSeconds);
            for (var s = 1; s <= totalSeconds; s++)
            {
                var delay = TimeSpan.FromSeconds(s);
                if (delay >= RcdDetonationDelay)
                    break;

                Timer.Spawn(delay, () =>
                {
                    if (!Exists(targetRcd))
                        return;
                    _audio.PlayPvs(RcdBeepSound, targetRcd);
                });
            }

            Timer.Spawn(RcdDetonationDelay, () =>
            {
                if (!Exists(targetRcd))
                    return;

                var currentXform = Transform(targetRcd);
                var coords = _xform.GetMapCoordinates(targetRcd, currentXform);
                _explosions.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
                    totalIntensity: 4f, slope: 1f, maxTileIntensity: 2f, cause: origin, maxTileBreak: 0);

                QueueDel(targetRcd);
            });
        }
    }
}
