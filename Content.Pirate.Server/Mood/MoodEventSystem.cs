using Content.Shared.Cuffs.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Mood;
using Content.Shared.Slippery;

namespace Content.Pirate.Server.Mood;

public sealed class MoodEventSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlipperyComponent, SlipEvent>(OnSlip);
        SubscribeLocalEvent<CuffableComponent, CuffedStateChangeEvent>(OnCuffedStateChanged);
        SubscribeLocalEvent<InteractionPopupComponent, InteractionSuccessEvent>(OnInteractionSuccess);
    }

    private void OnSlip(Entity<SlipperyComponent> entity, ref SlipEvent args)
    {
        RaiseLocalEvent(args.Slipped, new MoodEffectEvent("MobSlipped"));
    }

    private void OnCuffedStateChanged(Entity<CuffableComponent> entity, ref CuffedStateChangeEvent args)
    {
        if (entity.Comp.CanStillInteract)
            RaiseLocalEvent(entity.Owner, new MoodRemoveEffectEvent("Handcuffed"));
        else
            RaiseLocalEvent(entity.Owner, new MoodEffectEvent("Handcuffed"));
    }

    private void OnInteractionSuccess(Entity<InteractionPopupComponent> entity, ref InteractionSuccessEvent args)
    {
        if (entity.Comp.InteractSuccessString == "hugging-success-generic")
        {
            RaiseLocalEvent(entity.Owner, new MoodEffectEvent("BeingHugged"));
        }
        else if (entity.Comp.InteractSuccessString?.Contains("petting-success-", StringComparison.Ordinal) == true)
        {
            RaiseLocalEvent(args.User, new MoodEffectEvent("PetAnimal"));
        }
    }
}
