using Content.Server.Forensics;
using Content.Shared._Gobostation.Pinpointer;
using Content.Shared._Goobstation.Pinpointer;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Tag;

namespace Content.Server._Goobstation.Pinpointer;

public sealed partial class BloodtrakSystem : SharedBloodtrakSystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ForensicsSystem _forensicsSystem = default!;
    private EntityQuery<ForensicsComponent> _forensicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodtrakComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private EntityUid GetBloodTarget(EntityUid uid, BloodtrakComponent comp, AfterInteractEvent args)
    {
        if (args.Target == null || !_tag.HasTag((EntityUid)args.Target, "DNASolutionScannable"))
        {
            args.Handled = true;
            return default!;
        }

        _forensicsQuery = GetEntityQuery<ForensicsComponent>();
        _forensicsQuery.TryGetComponent(args.Target, out var forensics);

        if (forensics == null)
        {
            args.Handled = true;
            return default!;
        }

        foreach (var dna in _forensicsSystem.GetSolutionsDNA(args.Target.Value))
        {
            if (forensics.DNAs.Contains(dna))
                return forensics.Owner;
        }
        return default!;
    }

    /// <summary>
    ///     Set the target if capable
    /// </summary>
    private void OnAfterInteract(EntityUid uid, BloodtrakComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        if (component.IsActive)
            return;

        args.Handled = true;
        component.Target = GetBloodTarget(uid, component, args);
    }

    /// <summary>
    ///     Set pinpointers target to track
    /// </summary>
    public void SetTarget(EntityUid uid, EntityUid? target, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (pinpointer.Target == target)
            return;

        pinpointer.Target = target;
        if (pinpointer.IsActive)
            UpdateDirectionToTarget(uid, pinpointer);
    }
}
