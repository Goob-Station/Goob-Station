using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Explosion.Components;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Blob.Systems;

public sealed class BlobFactorySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobCoreComponent> _core;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobFactoryComponent, BlobSpecialGetPulseEvent>(OnPulsed);
        SubscribeLocalEvent<BlobFactoryComponent, ProduceBlobbernautEvent>(OnProduceBlobbernaut);
        SubscribeLocalEvent<BlobFactoryComponent, DestructionEventArgs>(OnDestruction);

        _tile = GetEntityQuery<BlobTileComponent>();
        _core = GetEntityQuery<BlobCoreComponent>();
    }

    private void OnDestruction(EntityUid uid, BlobFactoryComponent component, DestructionEventArgs args)
    {
        if (TryComp<BlobbernautComponent>(component.Blobbernaut, out var blobbernautComponent))
        {
            blobbernautComponent.Factory = null;
        }
    }

    private void OnProduceBlobbernaut(EntityUid uid, BlobFactoryComponent component, ProduceBlobbernautEvent args)
    {
        if (component.Blobbernaut != null)
            return;

        if (!_tile.TryComp(uid, out var blobTileComponent) || blobTileComponent.Core == null)
            return;

        if (!_core.TryComp(blobTileComponent.Core, out var blobCoreComponent))
            return;

        var xform = Transform(uid);

        var blobbernaut = Spawn(component.BlobbernautId, xform.Coordinates);

        component.Blobbernaut = blobbernaut;
        if (TryComp<BlobbernautComponent>(blobbernaut, out var blobbernautComponent))
        {
            blobbernautComponent.Factory = uid;
            blobbernautComponent.Color = blobCoreComponent.ChemСolors[blobCoreComponent.CurrentChem];
            Dirty(blobbernaut, blobbernautComponent);
        }
        if (TryComp<MeleeWeaponComponent>(blobbernaut, out var meleeWeaponComponent))
        {
            var blobbernautDamage = new DamageSpecifier();
            foreach (var keyValuePair in blobCoreComponent.ChemDamageDict[blobCoreComponent.CurrentChem].DamageDict)
            {
                blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
            }
            meleeWeaponComponent.Damage = blobbernautDamage;
        }
    }

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Phlogiston = "Phlogiston";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string TearGas = "TearGas";

    [ValidatePrototypeId<ReagentPrototype>]

    private const string Lexorin = "Lexorin";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Mold = "Mold";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Bicaridine = "Bicaridine";

    [ValidatePrototypeId<ReagentPrototype>]
    private const string Aluminium = "Aluminium";
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Iron = "Iron";
    [ValidatePrototypeId<ReagentPrototype>]
    private const string Uranium = "Uranium";

    private void FillSmokeGas(Entity<BlobPodComponent> ent, BlobChemType currentChem)
    {
        var blobGas = EnsureComp<SmokeOnTriggerComponent>(ent).Solution;
        switch (currentChem)
        {
            case BlobChemType.BlazingOil:
                blobGas.AddSolution(new Solution(Phlogiston, FixedPoint2.New(30))
                {
                    Temperature = 1000
                },
                    _prototypeManager);
                break;
            case BlobChemType.ReactiveSpines:
                blobGas.AddSolution(new Solution(Mold, FixedPoint2.New(30)),_prototypeManager);
                break;
            case BlobChemType.RegenerativeMateria:
                blobGas.AddSolution(new Solution(Bicaridine, FixedPoint2.New(30)),_prototypeManager);
                break;
            case BlobChemType.ExplosiveLattice:
                blobGas.AddSolution(new Solution(Lexorin, FixedPoint2.New(30))
                {
                    Temperature = 1000
                },
                    _prototypeManager);
                break;
            case BlobChemType.ElectromagneticWeb:
                blobGas.AddSolution(new Solution(Aluminium, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                blobGas.AddSolution(new Solution(Iron, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                blobGas.AddSolution(new Solution(Uranium, FixedPoint2.New(10)){ CanReact = false },_prototypeManager);
                break;
            default:
                blobGas.AddSolution(new Solution(TearGas, FixedPoint2.New(30)),_prototypeManager);
                break;
        }
    }

    private void OnPulsed(EntityUid uid, BlobFactoryComponent component, BlobSpecialGetPulseEvent args)
    {
        if (component.SpawnedCount >= component.MaxPods)
            return;

        if (component.Accumulator < component.AccumulateToSpawn)
        {
            component.Accumulator++;
            return;
        }

        if (!_tile.TryComp(uid, out var blobTileComponent) || blobTileComponent.Core == null)
            return;

        if (!_core.TryComp(blobTileComponent.Core, out var blobCoreComponent))
            return;

        var core = blobTileComponent.Core.Value;
        var xform = Transform(uid);

        var pod = Spawn(component.Pod, xform.Coordinates);
        component.BlobPods.Add(pod);
        var blobPod = EnsureComp<BlobPodComponent>(pod);
        blobPod.Core = core;
        FillSmokeGas((pod,blobPod), blobCoreComponent.CurrentChem);

        component.SpawnedCount += 1;
        component.Accumulator = 0;
    }
}
