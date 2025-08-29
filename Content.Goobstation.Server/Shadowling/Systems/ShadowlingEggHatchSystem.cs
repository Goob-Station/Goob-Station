using Content.Goobstation.Shared.Shadowling.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles the hatching process
/// </summary>
///
public sealed class ShadowlingEggHatchSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HatchingEggComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<HatchingEggComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.HasBeenHatched)
                continue;

            var shadowlingInside = comp.ShadowlingInside;

            if (shadowlingInside != null)
            {
                var sUid = shadowlingInside.Value;

                if (_timing.CurTime >= comp.NextUpdate)
                    Cycle(sUid, uid, comp);

                if (_timing.CurTime >= (comp.NextUpdate - TimeSpan.FromSeconds(12)) && !comp.HasFirstMessageAppeared)
                {
                    _popupSystem.PopupEntity(Loc.GetString("sling-hatch-first"), uid, sUid, PopupType.Medium);
                    _audio.PlayPvs(comp.CrackFirst, uid, AudioParams.Default.WithVolume(-2f));
                    comp.HasFirstMessageAppeared = true;
                }

                if (_timing.CurTime >= (comp.NextUpdate - TimeSpan.FromSeconds(7)) && !comp.HasSecondMessageAppeared)
                {
                    _popupSystem.PopupEntity(Loc.GetString("sling-hatch-second"), uid, sUid, PopupType.Medium);
                    _audio.PlayPvs(comp.CrackSecond, uid, AudioParams.Default.WithVolume(-2f));
                    comp.HasSecondMessageAppeared = true;
                }

                if (_timing.CurTime >= (comp.NextUpdate - TimeSpan.FromSeconds(3)) && !comp.HasThirdMessageAppeared)
                {
                    _popupSystem.PopupEntity(Loc.GetString("sling-hatch-third"), uid, sUid, PopupType.Medium);
                    _audio.PlayPvs(comp.CrackFirst, uid, AudioParams.Default.WithVolume(-2f).WithPitchScale(2f));
                    comp.HasThirdMessageAppeared = true;
                }
            }
        }
    }

    public void OnMapInit(Entity<HatchingEggComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.CooldownTimer;
    }

    public void Cycle(EntityUid sling, EntityUid egg, HatchingEggComponent comp)
    {
        if (!TryComp<ShadowlingComponent>(sling, out var shadowling))
            return;

        _audio.PlayPvs(comp.CrackThird, egg, AudioParams.Default.WithVolume(-2f));

        // Remove sling from egg
        if (TryComp<EntityStorageComponent>(egg, out var storage))
        {
            _entityStorage.Remove(sling, egg, storage);
            _entityStorage.OpenStorage(egg, storage);
        }

        var newUid = _polymorph.PolymorphEntity(sling, shadowling.ShadowlingPolymorphId);
        if (newUid == null)
            return;

        var ascendantShadowlingComp = EntityManager.GetComponent<ShadowlingComponent>(newUid.Value);
        var shadowlingSystem = EntityManager.System<ShadowlingSystem>();

        shadowlingSystem.OnPhaseChanged(newUid.Value, ascendantShadowlingComp, ShadowlingPhases.PostHatch);

        comp.HasBeenHatched = true;
    }
}
