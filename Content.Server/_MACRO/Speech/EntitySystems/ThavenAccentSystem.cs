using Content.Server._MACRO.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;

namespace Content.Server._MACRO.Speech.EntitySystems;

/// <summary>
///     this is a copy of NoContractionsAccentSystem, split to retain function of accentless for non thaven using the trait
/// </summary>
public sealed partial class ThavenAccentComponentAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThavenAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(Entity<ThavenAccentComponent> entity, ref AccentGetEvent args)
    {
        args.Message = _replacement.ApplyReplacements(args.Message, "nocontractions");
    }
}