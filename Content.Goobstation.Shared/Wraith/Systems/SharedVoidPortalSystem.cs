using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared.Examine;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed class SharedVoidPortalSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidPortalComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<VoidPortalComponent> ent, ref ExaminedEvent args)
    {
        // Only show info to wraith
        if (!HasComp<WraithComponent>(args.Examiner))
            return;

        // Display current portal power
        args.PushMarkup($"[color=mediumpurple]{Loc.GetString("void-portal-current-power", ("points", ent.Comp.CurrentPower))}[/color]");
        // Display current wave number
        args.PushMarkup($"[color=mediumpurple]{Loc.GetString("void-portal-current-wave", ("wave", ent.Comp.WavesCompleted))}[/color]");
    }
}
