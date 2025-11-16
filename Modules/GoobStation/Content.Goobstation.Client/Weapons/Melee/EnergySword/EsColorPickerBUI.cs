// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Melee.EnergySword;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Weapons.Melee.EnergySword
{
    [UsedImplicitly]
    public sealed class EsColorPickerBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

        private EsColorPicker? _window;
        private EntityUid _prototypeView;

        protected override void Open()
        {
            base.Open();
            _window = this.CreateWindow<EsColorPicker>();

            if (!EntMan.TryGetComponent<MetaDataComponent>(Owner, out var metadata) && metadata == null || metadata.EntityPrototype == null)
                return;
            _prototypeView = EntMan.Spawn(metadata.EntityPrototype.ID);

            _window.SetEntity(_prototypeView, Owner);
            _window.SetLogoAndFlavor(_prototypeView);

            _window.OnConfirmButtonPressed += color =>
            {
                SendPredictedMessage(new EsColorChangedMessage(color));
            };
            _window.OnSecretButtonPressed += state =>
            {
                SendPredictedMessage(new EsHackedStateChangedMessage(state));
            };
        }
    }
}
