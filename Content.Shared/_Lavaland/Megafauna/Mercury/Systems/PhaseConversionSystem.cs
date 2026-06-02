using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Switch forms mid fight.
/// </summary>
public sealed class PhaseConversionSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PhaseConversionComponent, PhaseConversionActionEvent>(OnAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PhaseConversionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.SwitchSoon)
                continue;

            comp.Accumulator += frameTime;

            if (comp.Accumulator < comp.SwitchDelay)
                continue;

            comp.Accumulator = 0f;
            comp.SwitchSoon = false;
            DoSwitch(uid, comp);
        }
    }

    private void OnAction(EntityUid uid, PhaseConversionComponent comp, PhaseConversionActionEvent args)
    {
        if (args.Handled || comp.SwitchSoon)
            return;

        _audio.PlayPredicted(comp.SwitchSound, uid, uid, AudioParams.Default.WithVolume(-5f));

        // Spawn in cool effect
        comp.EffectEntity = PredictedSpawnAttachedTo(comp.EffectPrototype, Transform(uid).Coordinates);
        if (comp.EffectEntity.HasValue)
        {
            _transform.SetParent(comp.EffectEntity.Value, uid);
        }

        comp.SwitchSoon = true;
        comp.Accumulator = 0f;
        args.Handled = true;
    }

    private void DoSwitch(EntityUid uid, PhaseConversionComponent comp)
    {
        comp.IsRanged = !comp.IsRanged;

        // Switch AI selector so it uses different actions
        if (TryComp<MegafaunaAiComponent>(uid, out var ai))
        {
            ProtoId<MegafaunaSelectorPrototype> selectorId;
            if (comp.IsRanged)
            {
                selectorId = comp.RangedSelector;
            }
            else
            {
                selectorId = comp.MeleeSelector;
            }
            ai.Selector = _protoMan.Index(selectorId).Selector;
        }

        // Switch sprite
        _appearance.SetData(uid, PhaseConversionVisuals.IsRanged, comp.IsRanged);
        _popup.PopupPredicted(Loc.GetString("ort-phase-conversion"), uid, uid, PopupType.Medium);
    }
}
