using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnBlind(BlindSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (_spectralQuery.HasComp(ev.Target))
            return;

        _statusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(ev.Target, "TemporaryBlindness", ev.BlindDuration, true);
        _statusEffects.TryAddStatusEffect<BlurryVisionComponent>(ev.Target, "BlurryVision", ev.BlurDuration, true);

        if (TryComp(ev.Target, out VocalComponent? vocal))
            _chat.TryEmoteWithChat(ev.Target, vocal.ScreamId);

        if (ev.Effect != null)
            PredictedSpawnAtPosition(ev.Effect.Value, Transform(ev.Target).Coordinates);

        ev.Handled = true;
    }
}