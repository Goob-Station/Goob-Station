// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bots;
using Content.Server.Animals.Components;
using Content.Server.Animals.Systems;
using Content.Server.Vocalization.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Sound.Components;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bots;

public sealed partial class AnnoyingDuckBotSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private EmagSystem _emag = default!;
    [Dependency] private EggLayerSystem _eggLayer = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    [SubscribeLocalEvent]
    private void OnEmagged(EntityUid uid, EmaggableAnnoyingDuckBotComponent comp, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        if (comp.Dataset != null && TryComp<DatasetVocalizerComponent>(uid, out var dataset))
            dataset.Dataset = comp.Dataset.Value;

        if (TryComp<SpamEmitSoundComponent>(uid, out var spam))
        {
            // Update the spam component with the new sound and interval settings.
            if (comp.Sound != null)
                spam.Sound = comp.Sound;

            spam.MinInterval = comp.MinSoundInterval;
            spam.MaxInterval = comp.MaxSoundInterval;
        }

        if (TryComp<EggLayerComponent>(uid, out var egg))
            _eggLayer.SetCooldowns((uid, egg), comp.EggLayCooldownMin, comp.EggLayCooldownMax);

        if (TryComp<VocalizerComponent>(uid, out var vocalizer))
        {
            vocalizer.MinVocalizeInterval = comp.MinVocalizeInterval;
            vocalizer.MaxVocalizeInterval = comp.MaxVocalizeInterval;

            // Reset the running countdown so the new interval applies immediately.
            var delay = _random.NextFloat((float) comp.MinVocalizeInterval.TotalSeconds,
                (float) comp.MaxVocalizeInterval.TotalSeconds);
            vocalizer.NextVocalizeInterval = _timing.CurTime + TimeSpan.FromSeconds(delay);
        }

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnHonk(EntityUid uid, EmaggableAnnoyingDuckBotComponent comp, ref AnnoyingDuckBotHonkEvent args)
    {
        if (comp.Sound != null)
            _audio.PlayPvs(comp.Sound, uid);
    }
}
