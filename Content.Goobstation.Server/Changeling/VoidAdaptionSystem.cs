using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class VoidAdaptionSystem : SharedVoidAdaptionSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<VoidAdaptionComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<VoidAdaptionComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<VoidAdaptionComponent>(ent, args.NewEntity);
    }
}
