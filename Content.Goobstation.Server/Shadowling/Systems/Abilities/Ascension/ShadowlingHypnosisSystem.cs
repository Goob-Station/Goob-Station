using Content.Server.Actions;
using Content.Shared._EE.Shadowling;
using Content.Shared.Humanoid;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Hypnosis.
/// Instant Thrall from afar!
/// </summary>
public sealed class ShadowlingHypnosisSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingHypnosisComponent, HypnosisEvent>(OnHypnosis);
    }

    private void OnHypnosis(EntityUid uid, ShadowlingHypnosisComponent component, HypnosisEvent args)
    {
        var target = args.Target;
        if (HasComp<ThrallComponent>(target) || HasComp<ShadowlingComponent>(target))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        EnsureComp<ThrallComponent>(target);
        _actions.StartUseDelay(args.Action);
    }
}
