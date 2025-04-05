using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Restrict;
public sealed partial class SharedRestrictSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RestrictInteractionByUserTagComponent, InteractionAttemptEvent>(OnAttemptInteract);
        SubscribeLocalEvent<RestrictMeleeByUserTagComponent, AttemptMeleeEvent>(OnAttemptMelee);
        SubscribeLocalEvent<RestrictGunshotsByUserTagComponent, ShotAttemptedEvent>(OnAttemptGunshot);
    }

    private void OnAttemptInteract(Entity<RestrictInteractionByUserTagComponent> ent, ref InteractionAttemptEvent args)
    {
        if (!_tagSystem.HasAllTags(args.Uid, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.Uid, ent.Comp.DoesntContain))
        {
            args.Cancelled = true;
            if (ent.Comp.Messages.Count != 0)
                _popup.PopupClient(Loc.GetString(_random.Pick(ent.Comp.Messages)), args.Uid);
        }
    }

    private void OnAttemptMelee(Entity<RestrictMeleeByUserTagComponent> ent, ref AttemptMeleeEvent args)
    {
        if(!_tagSystem.HasAllTags(args.User, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.User, ent.Comp.DoesntContain))
        {
            args.Cancelled = true;
            if(ent.Comp.Messages.Count != 0)
                args.Message = Loc.GetString(_random.Pick(ent.Comp.Messages));
        }
    }

    private void OnAttemptGunshot(Entity<RestrictGunshotsByUserTagComponent> ent, ref ShotAttemptedEvent args)
    {
        if(!_tagSystem.HasAllTags(args.User, ent.Comp.Contains) || _tagSystem.HasAnyTag(args.User, ent.Comp.DoesntContain))
        {
            args.Cancel();
            var time = _timing.CurTime;
            if(ent.Comp.Messages.Count != 0 && time > ent.Comp.LastPopup + TimeSpan.FromSeconds(1))
            {
                ent.Comp.LastPopup = time;
                _popup.PopupClient(Loc.GetString(_random.Pick(ent.Comp.Messages)), args.User);
            }
        }
    }
}
