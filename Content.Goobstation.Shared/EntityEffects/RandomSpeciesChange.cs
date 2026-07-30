// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class RandomSpeciesChange : EntityEffectBase<RandomSpeciesChange>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-species-random");
}

public sealed partial class RandomSpeciesChangeEffectSystem : EntityEffectSystem<HumanoidAppearanceComponent, RandomSpeciesChange>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedSpeciesChangeEffectSystem _speciesChange = default!;

    public static readonly HashSet<ProtoId<SpeciesPrototype>> SpeciesBlacklist = new()
    {
        "IPC",
        "Shadowling", // no ontag
        "Skeleton",
    };

    private List<ProtoId<SpeciesPrototype>> _species = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        LoadPrototypes();
    }

    protected override void Effect(Entity<HumanoidAppearanceComponent> ent, ref EntityEffectEvent<RandomSpeciesChange> args)
    {
        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
        var species = rand.Pick(_species);
        _speciesChange.Polymorph(ent, species);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<SpeciesPrototype>())
            LoadPrototypes();
    }

    private void LoadPrototypes()
    {
        _species.Clear();
        foreach (var species in _proto.EnumeratePrototypes<SpeciesPrototype>())
        {
            var id = new ProtoId<SpeciesPrototype>(species.ID);
            if (!SpeciesBlacklist.Contains(id))
                _species.Add(id);
        }
    }
}
