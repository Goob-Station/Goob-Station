using Content.Server._Reserve.Revolutionary.Components;
using Content.Server.EUI;
using Content.Server.GameTicking.Rules;
using Content.Server.Popups;
using Content.Shared._Reserve.Revolutionary;
using Content.Shared.Eui;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;

namespace Content.Server._Reserve.Revolutionary.UI;

public sealed class ConsentRequestedEui(EntityUid target, EntityUid converter, RevolutionaryRuleSystem revRuleSystem, ConsentRevolutionarySystem consRevSystem, PopupSystem popup, EntityManager entManager) : BaseEui
{
    public override EuiStateBase GetNewState()
    {
        return new ConsentRequestedState(Identity.Name(converter, entManager));
    }

    public override void Opened()
    {
        base.Opened();
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is ConsentRequestedEuiMessage consent && revRuleSystem.IsConvertable(target))
        {
            if (!entManager.TryGetComponent<ConsentRevolutionaryComponent>(target, out var targetConsRev)
                || !entManager.TryGetComponent<ConsentRevolutionaryComponent>(converter, out var consRev))
            {
                return;
            }

            if (consent.IsAccepted)
            {
                // Make target a revolutionary
                revRuleSystem.ConvertEntityToRevolution(target, converter);

                // Remove request
                consRevSystem.CancelRequest((target, targetConsRev), (converter, consRev));

                // Apply cooldown to convertor
                consRevSystem.ApplyConversionCooldown((converter, consRev));

                // Announce that convert was successful
                popup.PopupEntity(
                    Loc.GetString("rev-consent-convert-accepted", ("target", Identity.Entity(target, entManager))),
                    target,
                    converter);
            }
            else
            {
                // Cancel request with cooldown
                consRevSystem.CancelRequest((target, targetConsRev), (converter, consRev));

                // Apply conversion block to target
                consRevSystem.ApplyConversionDeny((target, targetConsRev));

                // Announce that convert failed
                popup.PopupEntity(
                    Loc.GetString("rev-consent-convert-denied", ("target", Identity.Entity(target, entManager))),
                    target,
                    converter,
                    PopupType.SmallCaution);
            }
        }

        Close();
    }
}
