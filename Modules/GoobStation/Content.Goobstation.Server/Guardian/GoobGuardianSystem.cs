// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Guardian;
using Content.Server.Guardian;
using Content.Shared.Actions;
using Content.Shared.Guardian;

namespace Content.Goobstation.Server.Guardian
{
    public sealed class GoobGuardianSystem : EntitySystem
    {
        [Dependency] private readonly GuardianSystem _guardian = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GuardianComponent, GuardianToggleSelfActionEvent>(OnPerformSelfAction); // Goobstation
        }

        private void OnPerformSelfAction(Entity<GuardianComponent> ent, ref GuardianToggleSelfActionEvent args)
        {
            if (ent.Comp.Host != null && TryComp<GuardianHostComponent>(ent.Comp.Host, out var hostComp) && ent.Comp.GuardianLoose)
                _guardian.ToggleGuardian(ent.Comp.Host.Value, hostComp);

            args.Handled = true;
        }
    }
}
