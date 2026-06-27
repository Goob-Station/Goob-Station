using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingRegenerateSystem : SharedChangelingRegenerateSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingRegenerateComponent, PolymorphedEvent>(OnPolymorphed);

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
    }

    private void OnPolymorphed(Entity<ChangelingRegenerateComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<ChangelingRegenerateComponent>(ent, args.NewEntity);
    }

    #region Helper Methods
    protected override void RegenerateChangelingBody(Entity<ChangelingRegenerateComponent> ent, BodyComponent bodyComp)
    {
        // this has to be done in server otherwise a shitload of warnings get thrown
        _body.RestoreBody((ent, bodyComp));
    }
    #endregion
}
