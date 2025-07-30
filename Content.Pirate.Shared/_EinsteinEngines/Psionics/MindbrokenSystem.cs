using Content.Shared.Examine;

namespace Content.Shared.Abilities.Psionics;

public sealed class MindbrokenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindbrokenComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, MindbrokenComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup($"[color=#f89b14]{Loc.GetString(component.MindbrokenExaminationText, ("entity", uid))}[/color]");
    }
}
