using Content.Goobstation.Shared.Boomerang;
using Content.Goobstation.Shared.Emag;
using Content.Shared.Emag.Systems;

namespace Content.Goobstation.Shared.Weapons.Grenades;

public sealed partial class GrenadesSystem : EntitySystem
{
    [Dependency] private readonly EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrenadeComponent, GotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<GrenadeComponent, EmagCleanedEvent>(OnEmagCleaned);
    }

    private void OnGotEmagged(Entity<GrenadeComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Jestographic))
            return;

        if (_emag.CheckFlag(ent.Owner, EmagType.Jestographic))
            return;

        EnsureComp<BoomerangComponent>(ent.Owner);

        args.Handled = true;
    }

    private void OnEmagCleaned(Entity<GrenadeComponent> ent, ref EmagCleanedEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<BoomerangComponent>(ent.Owner))
            return;

        RemComp<BoomerangComponent>(ent.Owner);

        args.Handled = true;
    }
}
