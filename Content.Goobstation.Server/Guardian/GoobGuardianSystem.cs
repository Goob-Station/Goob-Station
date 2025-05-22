// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Guardian;
using Content.Server.Guardian;
using Content.Shared.Actions;
using Content.Shared.Guardian;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Guardian
{
    public sealed class GoobGuardianSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly GuardianSystem _guardian = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GuardianComponent, GuardianToggleSelfActionEvent>(OnPerformSelfAction); // Goobstation
        }

        private void OnPerformSelfAction(EntityUid uid, GuardianComponent component, GuardianToggleSelfActionEvent args)
        {
            if (component.Host != null && TryComp<GuardianHostComponent>(component.Host, out var hostComp) && component.GuardianLoose)
                _guardian.ToggleGuardian(component.Host, hostComp);

            args.Handled = true;
        }
}
