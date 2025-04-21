using System.Linq;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Mind;
using Content.Shared.Players;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Map;

// todo
// speaking only to host
// randomized swapping
// split personality gamerule
namespace Content.Goobstation.Server.SplitPersonality;
public sealed partial class SplitPersonalitySystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SplitPersonalityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SplitPersonalityComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<SplitPersonalityDummyComponent, TakeGhostRoleEvent>(OnGhostRoleTaken);
    }

    private void OnInit(EntityUid uid, SplitPersonalityComponent comp, MapInitEvent args)
    {
        comp.MindsContainer = _container.EnsureContainer<Container>(uid, "SplitPersonalityContainer");
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        comp.MindRoles.AddRange(mind.MindRoles);

        for (var i = 0; i < comp.AdditionalMindsCount; i++)
        {
            // Spawn a dummy entity for each ghost role
            var dummy = Spawn("SplitPersonalityDummy", MapCoordinates.Nullspace);
            _container.Insert(dummy, comp.MindsContainer);
            comp.GhostRoleDummies.Add(dummy);

            // Setup ghost role component
            var ghostRole = EnsureComp<GhostRoleComponent>(dummy);
            ghostRole.RoleName = $"Split Personality of {MetaData(uid).EntityName}";
            ghostRole.RoleDescription = "A fragmented piece of the host's psyche.";

            _meta.SetEntityName(dummy, $"Split Personality of {MetaData(uid).EntityName}");

            mind.MindRoles.AddRange(comp.MindRoles);

            // Track the host entity on the dummy
            EnsureComp<SplitPersonalityDummyComponent>(dummy).Host = uid;
        }
    }

    private void OnGhostRoleTaken(EntityUid dummy, SplitPersonalityDummyComponent comp, TakeGhostRoleEvent args)
    {
        // Transfer the mind to visit the host entity
        if (args.Player.GetMind() is not { } mind)
            return;

        _mind.TransferTo(mind, dummy);
    }

    private void OnRemove(EntityUid uid, SplitPersonalityComponent comp, ComponentRemove args)
    {
        if (comp.OriginalMind is not { } originalMind)
            return;

        _mind.TransferTo(originalMind, uid);

        foreach (var dummy in comp.GhostRoleDummies.Where(dummy => !TerminatingOrDeleted(dummy)))
            QueueDel(dummy);

        comp.GhostRoleDummies.Clear();
    }
}
