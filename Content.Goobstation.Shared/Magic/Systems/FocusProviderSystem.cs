using Content.Shared.Examine;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Shared.Magic.Systems;

public sealed partial class FocusProviderSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicFocusProviderComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<MagicFocusProviderComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<MobStateComponent>(ent))
        {
            if (args.Examiner != ent.Owner)
                return;

            args.PushMarkup(Loc.GetString("magicitem-examine-self", ("n", ent.Comp.Weight)));
            return;
        }

        args.PushMarkup(Loc.GetString("magicitem-examine", ("n", ent.Comp.Weight)));
    }
}
