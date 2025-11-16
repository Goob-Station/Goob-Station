using Content.Goobstation.Shared.Wraith.Components;
using Content.Server.Ghost.Roles.Components;

namespace Content.Goobstation.Server.Wraith;

public sealed class WraithPossessedSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostRoleComponent, PossessionStartedEvent>(OnPossessionStarted);
        SubscribeLocalEvent<GhostRoleComponent, PossessionEndedEvent>(OnPossessionEnded);
    }

    private void OnPossessionStarted(Entity<GhostRoleComponent> ent, ref PossessionStartedEvent args) =>
        RemComp<GhostTakeoverAvailableComponent>(ent.Owner);

    private void OnPossessionEnded(Entity<GhostRoleComponent> ent, ref PossessionEndedEvent args) =>
        EnsureComp<GhostTakeoverAvailableComponent>(ent.Owner);
}
