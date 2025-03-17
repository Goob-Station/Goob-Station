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

namespace Content.Server._Goobstation.Security
{
    public sealed partial class PanicButtonSystem : EntitySystem
    {
        [Dependency] private readonly NavMapSystem _navMap = default!;
        [Dependency] private readonly RadioSystem _radioSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly UseDelaySystem _useDelaySystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PanicButtonComponent, UseInHandEvent>(OnButtonPressed);
            SubscribeLocalEvent<PanicButtonComponent, PanicButtonDoAfterEvent>(OnDoAfterComplete);
        }


        private void OnButtonPressed(Entity<PanicButtonComponent> ent, ref UseInHandEvent args)
        {
            if (!TryComp(ent.Owner, out UseDelayComponent? useDelay) ||
                _useDelaySystem.IsDelayed((ent.Owner, useDelay)))
            {
                args.Handled = true;
                return;
            }


            args.ApplyDelay = true;

            var comp = ent.Comp;
            var uid = args.User;

            var doAfterArgs = new DoAfterArgs(
                EntityManager,
                uid,
                comp.DoAfterDuration,
                new PanicButtonDoAfterEvent(),
                ent.Owner,
                ent.Owner,
                ent.Owner)
            {
                BreakOnMove = true,
                NeedHand = true,
                BlockDuplicate = true
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }

        private void OnDoAfterComplete(Entity<PanicButtonComponent> ent, ref PanicButtonDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled || !args.Target.HasValue)
                return;

            var comp = ent.Comp;
            var uid = ent.Owner;

            // Gets location of the implant
            var posText = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(uid));
            var distressMessage = Loc.GetString(comp.DistressMessage, ("position", posText));

            _radioSystem.SendRadioMessage(uid, distressMessage, _prototypeManager.Index<RadioChannelPrototype>(comp.RadioChannel), uid);
            args.Handled = true;

            if (!TryComp(ent.Owner, out UseDelayComponent? useDelay))
                return;

            _useDelaySystem.SetLength((uid, useDelay), comp.CoolDown, comp.DelayId);
            _useDelaySystem.TryResetDelay((uid, useDelay), id: comp.DelayId);




        }
    }
}
