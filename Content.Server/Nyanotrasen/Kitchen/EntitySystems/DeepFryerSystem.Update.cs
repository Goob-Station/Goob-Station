using System.Linq;
using Content.Server.Kitchen.Components;
using Content.Server.Nyanotrasen.Kitchen.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server.Nyanotrasen.Kitchen.EntitySystems;

public sealed partial class DeepFryerSystem
{
    public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var component in EntityManager.EntityQuery<DeepFryerComponent>())
            {
                var uid = component.Owner;

                if (_gameTimingSystem.CurTime < component.NextFryTime ||
                    !_powerReceiverSystem.IsPowered(uid))
                {
                    continue;
                }

                UpdateNextFryTime(uid, component);

                if (!_solutionContainerSystem.TryGetSolution(uid, component.Solution.Name, out var solution))
                    continue;

                // Heat the vat solution and contained entities.
                _solutionContainerSystem.SetTemperature(solution.Value, component.PoweredTemperature);

                foreach (var item in component.Storage.ContainedEntities)
                    CookItem(uid, component, item);

                // Do something bad if there's enough heat but not enough oil.
                var oilVolume = GetOilVolume(uid, component);

                if (oilVolume < component.SafeOilVolume)
                {
                    foreach (var item in component.Storage.ContainedEntities.ToArray())
                        BurnItem(uid, component, item);

                    if (oilVolume > FixedPoint2.Zero)
                    {
                        //JJ Comment - this code block makes the Linter fail, and doesn't seem to be necessary with the changes I made.
                        foreach (var reagent in component.Solution.Contents.ToArray())
                        {
                            _prototypeManager.TryIndex<ReagentPrototype>(reagent.Reagent.ToString(), out var proto);

                            foreach (var effect in component.UnsafeOilVolumeEffects)
                            {
                                var effectsArgs = new EntityEffectReagentArgs(uid,
                                    EntityManager,
                                    null,
                                    component.Solution,
                                    reagent.Quantity,
                                    proto!,
                                    null,
                                    1f);
                                effect.Effect(effectsArgs);
                            }

                        }

                        component.Solution.RemoveAllSolution();

                        _popupSystem.PopupEntity(
                            Loc.GetString("deep-fryer-oil-volume-low",
                                ("deepFryer", uid)),
                            uid,
                            PopupType.SmallCaution);

                        continue;
                    }
                }

                // We only alert the chef that there's a problem with oil purity
                // if there's anything to cook beyond this point.
                if (!component.Storage.ContainedEntities.Any())
                {
                    continue;
                }

                if (GetOilPurity(uid, component) < component.FryingOilThreshold)
                {
                    _popupSystem.PopupEntity(
                        Loc.GetString("deep-fryer-oil-purity-low",
                            ("deepFryer", uid)),
                        uid,
                        Filter.Pvs(uid, PvsWarningRange),
                        true);
                    continue;
                }

                foreach (var item in component.Storage.ContainedEntities.ToArray())
                    DeepFry(uid, component, item);

                // After the round of frying, replace the spent oil with a
                // waste product.
                if (component.WasteToAdd > FixedPoint2.Zero)
                {
                    foreach (var reagent in component.WasteReagents)
                        component.Solution.AddReagent(reagent.Reagent.ToString(), reagent.Quantity * component.WasteToAdd);

                    component.WasteToAdd = FixedPoint2.Zero;

                    _solutionContainerSystem.UpdateChemicals(solution.Value, true);
                }

                UpdateUserInterface(uid, component);
            }
        }

    private void UpdateAmbientSound(EntityUid uid, DeepFryerComponent component)
    {
        _ambientSoundSystem.SetAmbience(uid, HasBubblingOil(uid, component));
    }

    private void UpdateNextFryTime(EntityUid uid, DeepFryerComponent component)
    {
        component.NextFryTime = _gameTimingSystem.CurTime + component.FryInterval;
    }

}
