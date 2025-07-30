using Content.Shared.Psionics;
using Content.Shared.Eui;
using Content.Server.EUI;
using Content.Server.Abilities.Psionics;

namespace Content.Server.Psionics
{
    public sealed class AcceptPsionicsEui : BaseEui
    {
        private readonly PsionicAbilitiesSystem _psionicsSystem;
        private readonly EntityUid _entity;

        public AcceptPsionicsEui(EntityUid entity, PsionicAbilitiesSystem psionicsSys)
        {
            _entity = entity;
            _psionicsSystem = psionicsSys;
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is not AcceptPsionicsChoiceMessage choice ||
                choice.Button == AcceptPsionicsUiButton.Deny)
            {
                Close();
                return;
            }

            _psionicsSystem.AddRandomPsionicPower(_entity);
            Close();
        }
    }
}
