using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared.Examine;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class HauntedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HauntedComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, HauntedComponent comp, ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
        {
            args.PushMarkup($"[color=mediumpurple]{Loc.GetString("wraith-already-haunted", ("target", uid))}[/color]");
        }
    }
}
