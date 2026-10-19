using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.Server.MalfAI;

/// <summary>
/// Provides validation gating for shop-granted Shunt to APC and Return to Core actions, deferring
/// actual shunt logic to the MalfAiShuntSystem.
/// </summary>
public sealed class MalfAiShuntActionsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly Content.Server.Silicons.StationAi.StationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Content.Shared.Store.Components.StoreComponent, MalfAiShuntToApcActionEvent>(OnShuntToApcAction);
        SubscribeLocalEvent<Content.Shared.Store.Components.StoreComponent, MalfAiReturnToCoreActionEvent>(OnReturnToCoreAction);
    }

    private void OnShuntToApcAction(EntityUid uid, Content.Shared.Store.Components.StoreComponent comp, ref MalfAiShuntToApcActionEvent args)
    {
        // uid is the AI core/brain containing the store
        if (!HasComp<MalfAiMarkerComponent>(uid) || !HasComp<StationAiHeldComponent>(uid))
        {
            var performer = args.Performer != default ? args.Performer : uid;
            var popupTarget = GetAiEyeForPopup(uid) ?? performer;
            _popup.PopupEntity(Loc.GetString("malfai-shunt-invalid-user"), popupTarget, performer, PopupType.Medium);
            args.Handled = true;
            return;
        }
        // Allow MalfAiShuntSystem to handle valid actions.
    }

    private void OnReturnToCoreAction(EntityUid uid, Content.Shared.Store.Components.StoreComponent comp, ref MalfAiReturnToCoreActionEvent args)
    {
        // uid is the AI core/brain containing the store
        if (!HasComp<MalfAiMarkerComponent>(uid) || !HasComp<StationAiHeldComponent>(uid))
        {
            var performer = args.Performer != default ? args.Performer : uid;
            var popupTarget = GetAiEyeForPopup(uid) ?? performer;
            _popup.PopupEntity(Loc.GetString("malfai-return-invalid-user"), popupTarget, performer, PopupType.Medium);
            args.Handled = true;
            return;
        }
        // Allow MalfAiShuntSystem to handle valid actions.
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}
