using Content.Shared.Abilities.Psionics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly FlammableSystem _flammableSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PyrokinesisPowerActionEvent>(OnPowerUsed);
        }
        private void OnPowerUsed(PyrokinesisPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, "pyrokinesis", true))
                return;

            if (!TryComp<FlammableComponent>(args.Target, out var flammableComponent))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target)) // Pirate
                return;

            flammableComponent.FireStacks += 5;
            _flammableSystem.Ignite(args.Target, args.Target);
            _popupSystem.PopupEntity(Loc.GetString("pyrokinesis-power-used", ("target", args.Target)), args.Target, Shared.Popups.PopupType.LargeCaution);

            _psionics.LogPowerUsed(args.Performer, "pyrokinesis");
            args.Handled = true;
        }
    }
}
