using System.Linq;
using Content.Shared._Goobstation.Mood.Components;
using Content.Shared._Goobstation.Mood.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Mood.Systems;

public sealed class MobMoodSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobMoodComponent, DamageChangedEvent>(OnMobDamaged);
    }

    private void OnMobDamaged(Entity<MobMoodComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            ModifyMoodsForEvent(ent, args.DamageDelta!.GetTotal().Float(), MobMoodletModifierEventListenerId.Healed);
        else
            ModifyMoodsForEvent(ent, args.DamageDelta!.GetTotal().Float(), MobMoodletModifierEventListenerId.Damaged);
    }

    /// <summary>
    /// Change the value of a mood via an event. When a mood is normal, that means the event will add to the value, which will decay over time.
    /// If the mood is inverted, then the mood is always going up (generally a negative moodlet such as hunger) and will be decreased by this event.
    /// </summary>
    /// <param name="ent">The character entity experiencing mood</param>
    /// <param name="delta">The change in moodlet value. Should be a positive number.</param>
    /// <param name="eventId">The event that occurred causing a change.</param>
    private void ModifyMoodsForEvent(Entity<MobMoodComponent> ent, float delta, MobMoodletModifierEventListenerId eventId)
    {
        if (delta <= 0)
            return;

        foreach (var mood in ent.Comp.MobMoods)
        {
            RefreshMoodletValue(mood);

            if (mood.ModifierEvent == eventId)
            {
                // If inverted, the mood should decrease (upkeep). If not inverted, the mood should increase (bonus)
                mood.CurrentValue += mood.InvertedValue? -delta : delta;
            }

            mood.CurrentValue = float.Clamp(mood.CurrentValue, 0, mood.MaxValue);
        }
    }

    public float GetMoodValue(Entity<MobMoodComponent> entity, ProtoId<MobMoodletPrototype> moodPrototypeId)
    {
        if(entity.Comp.MobMoods.FirstOrDefault(o => o.ID == moodPrototypeId) is not { } mobMood)
            return float.NaN;

        RefreshMoodletValue(mobMood);
        return mobMood.CurrentValue;
    }

    // Applies decay to the value, ensuring that mood.CurrentValue is accurate after calling this function.
    private void RefreshMoodletValue(MobMoodletPrototype mobMood)
    {
        if (mobMood.LastChecked == null)
        {
            mobMood.LastChecked = DateTime.Now;
            return;
        }

        var deltaTime = DateTime.Now - mobMood.LastChecked;
        var decay = mobMood.ValueDecayPerSecond * (float)deltaTime.Value.TotalSeconds;

        if(mobMood.InvertedValue)
            // decay increases the value (moodlet needs to be actively maintained)
            mobMood.CurrentValue += decay;
        else
            // decay towards 0 (moodlet will vanish over time)
            mobMood.CurrentValue -= decay;

        mobMood.CurrentValue = float.Clamp(mobMood.CurrentValue, 0, mobMood.MaxValue);
    }

    public string GetLocalizedMoodDescription(Entity<MobMoodComponent> entity, ProtoId<MobMoodletPrototype> moodPrototypeId)
    {
        if(entity.Comp.MobMoods.FirstOrDefault(o => o.ID == moodPrototypeId) is not { } mobMood)
            return string.Empty;

        var moodValue = GetMoodValue(entity, moodPrototypeId);

        if (moodValue == 0)
            return Loc.GetString(mobMood.NeutralDescriptionLoc);

        var index = (int)MathF.Round((moodValue / mobMood.MaxValue) * (mobMood.DescriptionLocs.Length - 1));

        return Loc.GetString(mobMood.DescriptionLocs[index]);
    }
}
