// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Damage.Systems;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.EntityEffects;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.BloodCult.EntityEffects;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Runs Blood Cult reagent effects that need server logic.
/// </summary>
public sealed class BloodCultEntityEffectSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BloodCultMindShieldSystem _deconversion = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<BleedSanguinePerniculate>>(OnBleedSanguinePerniculate);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<DeCultify>>(OnDeCultify);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<CleanseCult>>(OnCleanseCult);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodCultistComponent, CleanseCultComponent>();
        while (query.MoveNext(out var uid, out _, out var cleanse))
        {
            if (_timing.CurTime < cleanse.CleanseTime)
                continue;

            RemComp<CleanseCultComponent>(uid);
            _deconversion.TryDeconvert(uid, popupLocId: null, stunDuration: TimeSpan.Zero, log: true);
        }
    }

    private void OnCleanseCult(ref ExecuteEntityEffectEvent<CleanseCult> args)
    {
        var target = args.Args.TargetEntity;
        if (HasComp<BloodCultistComponent>(target))
            EnsureComp<CleanseCultComponent>(target);
    }

    private void OnBleedSanguinePerniculate(ref ExecuteEntityEffectEvent<BleedSanguinePerniculate> args)
    {
        var target = args.Args.TargetEntity;
        if (!Exists(target))
            return;

        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return;

        if (!TryComp<EdgeEssentiaBloodComponent>(target, out var edgeEssentia))
        {
            edgeEssentia = AddComp<EdgeEssentiaBloodComponent>(target);
            if (!TryGetPrototypeBloodReagent(target, out var originalBlood))
                originalBlood = bloodstream.BloodReagent;

            edgeEssentia.OriginalBloodReagent = originalBlood;
        }

        _bloodstream.ChangeBloodReagent((target, bloodstream), "SanguinePerniculate");
    }

    private void OnDeCultify(ref ExecuteEntityEffectEvent<DeCultify> args)
    {
        var target = args.Args.TargetEntity;
        if (!TryComp(target, out BloodCultistComponent? bloodCultist))
            return;

        var scale = 1.0f;
        if (args.Args is EntityEffectReagentArgs reagentArgs)
            scale = reagentArgs.Scale.Float();

        var oldDeCultification = bloodCultist.DeCultification;
        var newDeCultification = oldDeCultification + args.Effect.Amount * scale;
        bloodCultist.DeCultification = newDeCultification;

        if (oldDeCultification >= 100.0f || newDeCultification < 100.0f)
            return;

        _audio.PlayPvs(
            new SoundPathSpecifier("/Audio/Effects/holy.ogg"),
            target,
            AudioParams.Default);

        _stamina.TakeStaminaDamage(target, 100f, visual: false);
    }

    private bool TryGetPrototypeBloodReagent(EntityUid uid, out ProtoId<ReagentPrototype> bloodReagent)
    {
        bloodReagent = default!;

        if (!TryComp<MetaDataComponent>(uid, out var meta) || meta.EntityPrototype == null)
            return false;

        if (!meta.EntityPrototype.TryGetComponent(_componentFactory.GetComponentName<BloodstreamComponent>(),
                out BloodstreamComponent? prototypeBloodstream))
            return false;

        bloodReagent = prototypeBloodstream.BloodReagent;
        return true;
    }
}
