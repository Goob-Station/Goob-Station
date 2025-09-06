using CVars = Content.Shared._CorvaxGoob.CCCVars.CCCVars;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Shared._CorvaxGoob.Jobs;

public sealed partial class MrpJobSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();

        ApplyMrpJobPreferences();
        InitializeMrpSystem();
    }

    private void ApplyMrpJobPreferences()
    {
        var mrpEnabled = _cfg.GetCVar(CVars.MrpJobsEnabled);

        foreach (var department in _prototypes.EnumeratePrototypes<DepartmentPrototype>())
        {
            department.Roles.RemoveAll(jobId =>
            {
                if (!_prototypes.TryIndex(jobId, out JobPrototype? proto))
                    return false;

                // Neutral (null) => never hide via this system
                if (proto.Mrp is null)
                    return false;

                // When MRP is enabled, hide explicit false; when disabled, hide explicit true
                return mrpEnabled ? proto.Mrp == false : proto.Mrp == true;
            });
        }
    }

    partial void InitializeMrpSystem();
}
