using Content.Server.Silicons.StationAi;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store.Components;
using Content.Shared._Gabystation.MalfAi.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Provides validation gating for shop-granted Shunt to APC and Return to Core actions, deferring
/// actual shunt logic to the MalfAiShuntSystem.
/// </summary>
public sealed class MalfAiShuntActionsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreComponent, MalfAiShuntToApcActionEvent>(OnShuntToApcAction);
        SubscribeLocalEvent<StoreComponent, MalfAiReturnToCoreActionEvent>(OnReturnToCoreAction);
    }

    private void OnShuntToApcAction(EntityUid uid, StoreComponent comp, ref MalfAiShuntToApcActionEvent args)
    {
        var performer = args.Performer != default ? args.Performer : uid;
        if (!HasComp<MalfunctioningAiComponent>(performer) || !HasComp<StationAiHeldComponent>(performer))
        {
            var popupTarget = GetAiEyeForPopup(performer) ?? performer;
            _popup.PopupEntity(Loc.GetString("malfai-shunt-invalid-user"), popupTarget, performer, PopupType.Medium);
            args.Handled = true;
            return;
        }
        // Allow MalfAiShuntSystem to handle valid actions.
    }

    private void OnReturnToCoreAction(EntityUid uid, StoreComponent comp, ref MalfAiReturnToCoreActionEvent args)
    {
        var performer = args.Performer != default ? args.Performer : uid;
        if (!HasComp<MalfunctioningAiComponent>(performer) || !HasComp<StationAiHeldComponent>(performer))
        {
            var popupTarget = GetAiEyeForPopup(performer) ?? performer;
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
