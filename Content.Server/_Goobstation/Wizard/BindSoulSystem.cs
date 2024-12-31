using Content.Server.Administration.Commands;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Server._Goobstation.Wizard;

public sealed class BindSoulSystem : SharedBindSoulSystem
{
    public override void Resurrect(EntityUid mind,
        EntityUid phylactery,
        MindComponent mindComp,
        SoulBoundComponent soulBound)
    {
        base.Resurrect(mind, phylactery, mindComp, soulBound);

        var ent = Spawn(LichPrototype, TransformSystem.GetMapCoordinates(phylactery));
        Mind.TransferTo(mind, ent, mind: mindComp);

        RemCompDeferred<TransferMindOnGibComponent>(ent);
        EnsureComp<WizardComponent>(ent);

        SetOutfitCommand.SetOutfit(ent, LichGear, EntityManager);

        if (soulBound.Name != string.Empty)
            Meta.SetEntityName(ent, soulBound.Name);

        Stun.TryKnockdown(ent, TimeSpan.FromSeconds(20) + TimeSpan.FromSeconds(5) * soulBound.ResurrectionsCount, true);
        soulBound.ResurrectionsCount++;
        Dirty(mind, soulBound);
    }

    protected override void MakeDestructible(EntityUid uid)
    {
        base.MakeDestructible(uid);

        var destructible = EnsureComp<DestructibleComponent>(uid);
        var trigger = new DamageTrigger
        {
            Damage = 200,
        };
        var behavior = new DoActsBehavior
        {
            Acts = ThresholdActs.Destruction,
        };
        var threshold = new DamageThreshold
        {
            Trigger = trigger,
            Behaviors = new() { behavior },
        };
        destructible.Thresholds.Add(threshold);
    }
}
