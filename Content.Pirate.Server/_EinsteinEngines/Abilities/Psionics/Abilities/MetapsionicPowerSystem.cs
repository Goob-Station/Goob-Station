using Content.Shared.Abilities.Psionics;
using Content.Shared.Popups;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class MetapsionicPowerSystem : EntitySystem
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MetapsionicPowerComponent, MetapsionicPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(EntityUid uid, MetapsionicPowerComponent component, MetapsionicPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, "metapsionic pulse"))
                return;

            foreach (var entity in _lookup.GetEntitiesInRange(uid, component.Range))
            {
                if (HasComp<PsionicComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity) &&
                    !(HasComp<ClothingGrantPsionicPowerComponent>(entity) && Transform(entity).ParentUid == uid))
                {
                    _popups.PopupEntity(Loc.GetString("metapsionic-pulse-success"), uid, uid, PopupType.LargeCaution);
                    args.Handled = true;
                    return;
                }
            }
            _popups.PopupEntity(Loc.GetString("metapsionic-pulse-failure"), uid, uid, PopupType.Large);
            _psionics.LogPowerUsed(uid, "metapsionic pulse", 2, 4);

            args.Handled = true;
        }
    }
}
