using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.UserInterface;

namespace Content.Shared._Funkystation.MalfAI.Shunt;

/// <summary>
/// Malf Ai APC siphon interactions prediction.
/// </summary>
public sealed class SharedMalfAiApcSiphonSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiApcSiphonedComponent, InteractHandEvent>(OnSiphonedInteract);
        SubscribeLocalEvent<MalfAiApcSiphonedComponent, ActivatableUIOpenAttemptEvent>(OnSiphonedUIOpenAttempt);
    }

    private void OnSiphonedInteract(EntityUid uid, MalfAiApcSiphonedComponent siphoned, InteractHandEvent args)
    {
        _popup.PopupPredictedCursor(Loc.GetString("malfai-apc-unresponsive"), args.User);
        args.Handled = true;
    }

    private void OnSiphonedUIOpenAttempt(EntityUid uid, MalfAiApcSiphonedComponent siphoned, ActivatableUIOpenAttemptEvent args)
    {
        _popup.PopupPredictedCursor(Loc.GetString("malfai-apc-unresponsive"), args.User);
        args.Cancel();
    }
}
