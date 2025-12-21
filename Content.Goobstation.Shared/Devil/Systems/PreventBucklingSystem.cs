using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Devil.Systems;

public sealed class PreventBucklingSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventBucklingComponent, BuckleAttemptEvent>(OnBuckleAttempt);
    }

    private void OnBuckleAttempt(EntityUid uid, PreventBucklingComponent component, ref BuckleAttemptEvent args)
    {
        args.Cancelled = true;

        _popup.PopupPredicted(Loc.GetString("prevent-buckling-message"), uid, uid);
    }
}
