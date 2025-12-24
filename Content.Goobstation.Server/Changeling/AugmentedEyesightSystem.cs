using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class AugmentedEyesightSystem : SharedAugmentedEyesightSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;
    private EntityQuery<EyeComponent> _eyeQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentedEyesightComponent, PolymorphedEvent>(OnPolymorphed);

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
        _eyeQuery = GetEntityQuery<EyeComponent>();
    }

    private void OnPolymorphed(Entity<AugmentedEyesightComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<AugmentedEyesightComponent>(ent, args.NewEntity);
    }

    // has to be done in server otherwise fov flickering happens
    protected override void SetVision(Entity<AugmentedEyesightComponent> ent, bool? state = null)
    {
        if (!_eyeQuery.TryComp(ent, out var eyeComp))
            return;

        _eye.SetDrawFov(ent, state ?? ent.Comp.Enabled, eyeComp);
    }
}
