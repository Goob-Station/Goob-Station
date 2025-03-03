using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Medical.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.Medical.Cryogenics;

namespace Content.Server.Medical
{
    public sealed partial class CryoPodSystem
    {
        public override void InitializeInsideCryoPod()
        {
            base.InitializeInsideCryoPod();
            // Atmos overrides
            SubscribeLocalEvent<InsideCryoPodComponent, InhaleLocationEvent>(OnInhaleLocation);
            SubscribeLocalEvent<InsideCryoPodComponent, ExhaleLocationEvent>(OnExhaleLocation);
            SubscribeLocalEvent<InsideCryoPodComponent, AtmosExposedGetAirEvent>(OnGetAir);
            // Shitmed Change Start
            SubscribeLocalEvent<InsideCryoPodComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<InsideCryoPodComponent, ComponentRemove>(OnComponentRemove);
        }
        private void OnComponentInit(EntityUid uid, InsideCryoPodComponent component, ComponentInit args)
        {
            _actionsSystem.AddAction(uid, ref component.SleepAction, SleepingSystem.SleepActionId, uid);
        }

        private void OnComponentRemove(EntityUid uid, InsideCryoPodComponent component, ComponentRemove args)
        {
            _actionsSystem.RemoveAction(uid, component.SleepAction);
            _sleepingSystem.TryWaking(uid);
        }
        // Shitmed Change End

        #region Atmos handlers

        private void OnGetAir(EntityUid uid, InsideCryoPodComponent component, ref AtmosExposedGetAirEvent args)
        {
            if (TryComp<CryoPodAirComponent>(Transform(uid).ParentUid, out var cryoPodAir))
            {
                args.Gas = cryoPodAir.Air;
                args.Handled = true;
            }
        }

        private void OnInhaleLocation(EntityUid uid, InsideCryoPodComponent component, InhaleLocationEvent args)
        {
            if (TryComp<CryoPodAirComponent>(Transform(uid).ParentUid, out var cryoPodAir))
            {
                args.Gas = cryoPodAir.Air;
            }
        }

        private void OnExhaleLocation(EntityUid uid, InsideCryoPodComponent component, ExhaleLocationEvent args)
        {
            if (TryComp<CryoPodAirComponent>(Transform(uid).ParentUid, out var cryoPodAir))
            {
                args.Gas = cryoPodAir.Air;
            }
        }

        #endregion
    }
}
