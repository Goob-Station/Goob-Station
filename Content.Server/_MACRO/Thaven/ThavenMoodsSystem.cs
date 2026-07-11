using Content.Server._MACRO.StrangeMoods;
using Content.Server.StationEvents.Events;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared._MACRO.Species.Thaven;
using Content.Shared.Emag.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MACRO.Thaven;

/// <inheritdoc/>
public sealed partial class ThavenMoodsSystem : SharedThavenMoodsSystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StrangeMoodsSystem _moods = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThavenMoodsComponent, IonStormEvent>(OnIonStorm);
    }

    private void OnIonStorm(Entity<ThavenMoodsComponent> ent, ref IonStormEvent args)
    {
        if (!ent.Comp.IonStormable || !TryComp<StrangeMoodsComponent>(ent, out var moodsComp))
            return;

        var moods = moodsComp.StrangeMood.Moods;

        // remove mood
        if (_random.Prob(ent.Comp.IonStormRemoveChance) && moods.Count > 1)
        {
            var removeIndex = _random.Next(moods.Count);
            moods.RemoveAt(removeIndex);
            Dirty(ent);

            _moods.NotifyMoodChange((ent, moodsComp));
        }

        // add mood
        else if (_random.Prob(ent.Comp.IonStormAddChance) && moods.Count < ent.Comp.MaxIonMoods)
            _moods.TryAddRandomMood((ent, moodsComp), ent.Comp.IonStormDataset);

        // replace mood
        else
        {
            var conflicts = new HashSet<ProtoId<StrangeMoodPrototype>>();

            if (moods.Count > 1)
            {
                var removeIndex = _random.Next(moods.Count);

                if (moods[removeIndex].ProtoId is { } moodProto)
                    conflicts.Add(moodProto);

                moods.RemoveAt(removeIndex);
            }

            _moods.TryAddRandomMood((ent, moodsComp), ent.Comp.IonStormDataset, conflicts: conflicts);
        }
    }

    protected override void OnEmagged(Entity<ThavenMoodsComponent> ent, ref GotEmaggedEvent args)
    {
        base.OnEmagged(ent, ref args);

        if (!args.Handled || !TryComp<StrangeMoodsComponent>(ent, out var moods))
            return;

        _moods.TryAddRandomMood((ent, moods), ent.Comp.WildcardDataset);
    }
}