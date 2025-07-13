using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.Audio;

namespace Content.Pirate.Server.Traits.LightStep;

public sealed class LightStepSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightStepComponent, MoveEvent>(OnMove);
    }

    private void OnMove(Entity<LightStepComponent> ent, ref MoveEvent args)
    {
        if (EnsureComp<FootstepModifierComponent>(ent.Owner, out var footstep))
        {
            UpdateCurrentStepSoundCollection(ent);
            footstep.FootstepSoundCollection = ent.Comp.CurrentStepCollection;
            footstep.FootstepSoundCollection.Params = footstep.FootstepSoundCollection.Params.WithVolume(ent.Comp.Volume);
        }
    }

    private void UpdateCurrentStepSoundCollection(Entity<LightStepComponent> ent)
    {
        var ev = new GetFootstepSoundEvent(ent.Owner);
        RaiseLocalEvent(ent.Owner, ref ev);

        if (ev.Sound is SoundCollectionSpecifier collection)
            ent.Comp.CurrentStepCollection = collection;
    }
}
