using Content.Client.Labels.RenameSystem;
using Content.Shared.HandRenamer;
using Robust.Client.UserInterface;

namespace Content.Client.Labels.UI
{
    public sealed class HandRenamerBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        [ViewVariables]
        private HandRenamerWindow? _window;

        public HandRenamerBoundUserInterface(EntityUid owner, Enum uikey) : base(owner, uikey)
        {
            IoCManager.InjectDependencies(this);
        }

        protected override void Open()
        {
            base.Open();
            _window = this.CreateWindow<HandRenamerWindow>();
            if (_entManager.TryGetComponent(Owner, out HandRenamerComponent? renamer))
            {
                _window.SetMaxNameLength(renamer!.MaxNameChars);
            }

            _window.OnNameChanged += OnNameChanged;
            Reload();
        }

        private void OnNameChanged(string newName)
        {
            if (_entManager.TryGetComponent(Owner, out HandRenamerComponent? component) &&
                component.AssignedName.Equals(newName))
                return;

            SendPredictedMessage(new HandRenamerNameChangedMessage(newName));
        }

        public void Reload()
        {
            if (_window == null || !_entManager.TryGetComponent(Owner, out HandRenamerComponent? component))
                return;

            _window.SetCurrentName(component.AssignedName);
        }
    }
}
