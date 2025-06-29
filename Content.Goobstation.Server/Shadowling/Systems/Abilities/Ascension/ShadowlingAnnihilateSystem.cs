using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Actions;
using Content.Shared.Gibbing.Events;
using Content.Server.Body.Systems;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Annihilate abiltiy logic.
/// Gib from afar!
/// </summary>
public sealed class ShadowlingAnnihilateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAnnihilateComponent, AnnihilateEvent>(OnAnnihilate);
    }

    private void OnAnnihilate(EntityUid uid, ShadowlingAnnihilateComponent component, AnnihilateEvent args)
    {
        // The gibbening
        var target = args.Target;
        if (HasComp<ShadowlingComponent>(target))
            return;

        _body.GibBody(target, contents: GibContentsOption.Gib);

        _actions.StartUseDelay(args.Action);
    }
}
