using Content.Client.Eui;
using Content.Shared.Psionics;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.Psionics.UI
{
    [UsedImplicitly]
    public sealed class AcceptPsionicsEui : BaseEui
    {
        private readonly AcceptPsionicsWindow _window;

        public AcceptPsionicsEui()
        {
            _window = new AcceptPsionicsWindow();

            _window.DenyButton.OnPressed += _ =>
            {
                SendMessage(new AcceptPsionicsChoiceMessage(AcceptPsionicsUiButton.Deny));
                _window.Close();
            };

            _window.AcceptButton.OnPressed += _ =>
            {
                SendMessage(new AcceptPsionicsChoiceMessage(AcceptPsionicsUiButton.Accept));
                _window.Close();
            };
        }

        public override void Opened()
        {
            IoCManager.Resolve<IClyde>().RequestWindowAttention();
            _window.OpenCentered();
        }

        public override void Closed()
        {
            _window.Close();
        }

    }
}
