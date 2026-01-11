using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

public abstract class SharedXenobiologySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlimeComponent, InteractionSuccessEvent>(OnInteractionSuccess);
        SubscribeLocalEvent<SlimeComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<SlimeComponent> slime, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;
        if (slime.Comp.Tamer == args.Examiner)
            args.PushMarkup(Loc.GetString("slime-examined-tamer"));
        if (slime.Comp.Stomach.Count > 0)
            args.PushMarkup(Loc.GetString("slime-examined-stomach"));
    }

    private void OnInteractionSuccess(Entity<SlimeComponent> ent, ref InteractionSuccessEvent args)
    {
        if (ent.Comp.Tamer.HasValue)
        {
            _popup.PopupPredicted(Loc.GetString("slime-interaction-tame-fail"), args.User, args.User);
            return;
        }

        var coords = Transform(ent).Coordinates;

        // Hearts VFX - Slime taming is seperate to core Pettable Component/System
        PredictedSpawnAtPosition(ent.Comp.TameEffect, coords);
        ent.Comp.Tamer = args.User;

        _popup.PopupPredicted(Loc.GetString("slime-interaction-tame"), args.User, args.User);
    }
}
