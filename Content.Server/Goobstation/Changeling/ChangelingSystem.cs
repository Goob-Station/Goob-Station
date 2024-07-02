using Content.Server.Store.Systems;
using Content.Shared.Changeling;
using Content.Shared.Store.Components;

namespace Content.Server.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);

        SubscribeLocalEvent<ChangelingComponent, AbsorbDNAEvent>(OnAbsorbDNA);
        SubscribeLocalEvent<ChangelingComponent, ChangelingTransformEvent>(OnTransform);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var ling in EntityManager.EntityQuery<ChangelingComponent>())
        {
            ling.ChemicalRegenerationAccumulator += frameTime;

            if (ling.ChemicalRegenerationAccumulator >= ling.ChemicalRegenerationTimer)
                UpdateChemicals(ling);
        }
    }

    private void UpdateChemicals(ChangelingComponent comp)
    {
        comp.Chemicals += 1 / (1 + comp.ChemicalRegenerationModifier);
    }

    public void ApplyChemicalModifier(ChangelingComponent comp, float modifier)
    {
        comp.ChemicalRegenerationModifier += modifier;
    }

    #region Event Handlers

    private void OnOpenEvolutionMenu(EntityUid uid, ChangelingComponent comp, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnAbsorbDNA(EntityUid uid, ChangelingComponent comp, ref AbsorbDNAEvent args)
    {

    }

    private void OnTransform(EntityUid uid, ChangelingComponent comp, ref ChangelingTransformEvent args)
    {

    }

    #endregion
}
