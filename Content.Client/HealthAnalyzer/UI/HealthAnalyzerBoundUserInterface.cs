using Content.Shared.MedicalScanner;
using Content.Shared._Shitmed.Targeting; // Shitmed Change
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.HealthAnalyzer.UI
{
    [UsedImplicitly]
    public sealed class HealthAnalyzerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private HealthAnalyzerWindow? _window;

        public HealthAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<HealthAnalyzerWindow>();
            _window.OnBodyPartSelected += SendBodyPartMessage; // Shitmed Change
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        }


        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not HealthAnalyzerScannedUserMessage cast)
                return;

            _window.Populate(cast);
        }

        // Shitmed Change Start
        private void SendBodyPartMessage(TargetBodyPart? part, EntityUid target) => SendMessage(new HealthAnalyzerPartMessage(EntMan.GetNetEntity(target), part ?? null));
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_window != null)
                _window.OnBodyPartSelected -= SendBodyPartMessage;

            _window?.Dispose();
        }

        // Shitmed Change End
    }
}
