using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Content.Shared._Funkystation.Atmos.Components;

namespace Content.Client._Funkystation.Atmos.UI
{
    [UsedImplicitly]
    public sealed class CrystallizerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private CrystallizerWindow? _window;

        public CrystallizerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<CrystallizerWindow>();
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

            _window.ToggleStatusButton.OnPressed += _ => OnToggleStatusButtonPressed();
            _window.OnRecipeButtonPressed += (button, recipeId) => SendMessage(new CrystallizerSelectRecipeMessage(recipeId));
            _window.OnGasInputChanged += gasInput => SendMessage(new CrystallizerSetGasInputMessage(gasInput));
        }

        private void OnToggleStatusButtonPressed()
        {
            if (_window is null) return;

            _window.SetActive(!_window.Active);
            SendMessage(new CrystallizerToggleMessage());
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is CrystallizerUpdateGasMixtureMessage gasMessage)
            {
                _window.SetGasMixture(gasMessage.GasMixture);
            }

            if (message is CrystallizerProgressBarMessage progressBar)
            {
                _window.SetProgressBar(progressBar.ProgressBar);
            }
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not CrystallizerBoundUserInterfaceState cast)
                return;

            _window.SetActive(cast.Enabled);
            _window.SelectRecipeById(cast.SelectedRecipeId);
            _window.GasInput.Value = cast.GasInput;
            _window.SetGasMixture(cast.GasMixture);
        }
    }
}
