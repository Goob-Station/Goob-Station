using Content.Server.Ghost;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Chat;

/// <summary>
/// Handles completion of the suicide do-after and performs the ghost/kill logic.
/// </summary>
public sealed class SuicideDoAfterSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly GhostSystem _ghostSystem = default!;

    private static readonly ProtoId<TagPrototype> CannotSuicideTag = "CannotSuicide";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateComponent, SuicideDoAfterEvent>(OnSuicideDoAfter);
    }

    private void OnSuicideDoAfter(Entity<MobStateComponent> victim, ref SuicideDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        // Attempt to ghost the mind first
        var ghostEv = new SuicideGhostEvent(victim);
        RaiseLocalEvent(victim, ghostEv);

        // If ghosting failed, do nothing
        if (!ghostEv.Handled)
            return;

        // If they have the CannotSuicide tag, we only ghost them and do not kill the body
        if (_tagSystem.HasTag(victim, CannotSuicideTag))
            return;

        // Proceed with suicide
        var suicideEvent = new SuicideEvent(victim);
        RaiseLocalEvent(victim, suicideEvent);

        args.Handled = true;
    }
}
