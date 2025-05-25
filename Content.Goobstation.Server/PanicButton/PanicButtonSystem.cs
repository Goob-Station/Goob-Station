// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DoAfter;
using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Goobstation.Security;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Radio;
using Content.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.PanicButton
{
    public sealed partial class PanicButtonSystem : EntitySystem
    {
        [Dependency] private readonly NavMapSystem _navMap = default!;
        [Dependency] private readonly RadioSystem _radioSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly UseDelaySystem _useDelaySystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PanicButtonComponent, UseInHandEvent>(OnButtonPressed);
        }

        private void OnButtonPressed(Entity<PanicButtonComponent> ent, ref UseInHandEvent args)
        {
            if (args.Handled)
                return;

            EnsureComp<UseDelayComponent>(ent.Owner, out var useDelay);
            if (_useDelaySystem.IsDelayed((ent.Owner, useDelay)))
                return;

            var comp = ent.Comp;
            var uid = ent.Owner;

            if (_useDelaySystem.IsDelayed((ent.Owner, useDelay)))
                return;

            _useDelaySystem.TryResetDelay((uid, useDelay));

            // Gets location of the implant
            var posText = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(uid));
            var distressMessage = Loc.GetString(comp.DistressMessage, ("position", posText));

            _radioSystem.SendRadioMessage(uid, distressMessage, _prototypeManager.Index<RadioChannelPrototype>(comp.RadioChannel), uid);

            args.Handled = true;
        }
    }
}
