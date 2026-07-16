// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Humanoid;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Humanoid.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Shared.Polymorph.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Polymorph.Components;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SpeciesChangeSystem : EntityEffectSystem<HumanoidAppearanceComponent, SpeciesChange>
{
    protected override void Effect(Entity<HumanoidAppearanceComponent> entity, ref EntityEffectEvent<SpeciesChange> args)
    {
        var ev = new SpeciesChange(args.Effect.NewSpecies, args.Effect.TransferAppearance);
        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

[UsedImplicitly]
public sealed partial class SpeciesChange : EntityEffectBase<SpeciesChange>
{
    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> NewSpecies;

    /// <summary>
    ///     Keep the original look (skin, eyes, hair, sex, height...) where the new species allows it.
    /// </summary>
    [DataField]
    public bool TransferAppearance;

    public SpeciesChange() { }

    public SpeciesChange(ProtoId<SpeciesPrototype> newspecies, bool transferAppearance = false)
    {
        NewSpecies = newspecies;
        TransferAppearance = transferAppearance;
    }

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-species", ("species", NewSpecies));
}
