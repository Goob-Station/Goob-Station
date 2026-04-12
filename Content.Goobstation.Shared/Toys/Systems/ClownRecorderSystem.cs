using Content.Goobstation.Common.Emag;
using Content.Goobstation.Shared.Toys.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Timing;

namespace Content.Goobstation.Shared.Toys.Systems;
public sealed partial class ClownRecorderSystem : EntitySystem
{
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClownRecorderComponent, GotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<ClownRecorderComponent, EmagCleanedEvent>(OnEmagCleaned);
    }

    private void OnGotEmagged(Entity<ClownRecorderComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Jestographic))
            return;

        if (_emag.CheckFlag(ent.Owner, EmagType.Jestographic))
            return;

        if (!TryComp<UseDelayComponent>(ent.Owner, out var useDelay))
            return;

        var entity = (ent.Owner, useDelay);

        _useDelay.SetLength(entity, ent.Comp.EmaggedDelay);

        args.Handled = true;
    }

    private void OnEmagCleaned(Entity<ClownRecorderComponent> ent, ref EmagCleanedEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<UseDelayComponent>(ent.Owner, out var useDelay))
            return;

        var entity = (ent.Owner, useDelay);
        _useDelay.SetLength(entity, ent.Comp.NormalDelay);

        args.Handled = true;
    }
}
