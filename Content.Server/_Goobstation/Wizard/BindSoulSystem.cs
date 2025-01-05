using Content.Server.Administration.Commands;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.Respawn;
using Content.Server.Station.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Server._Goobstation.Wizard;

public sealed class BindSoulSystem : SharedBindSoulSystem
{
    [Dependency] private readonly SpecialRespawnSystem _respawn = default!;
    [Dependency] private readonly WizardRuleSystem _wizard = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Resurrect(EntityUid mind,
        EntityUid phylactery,
        MindComponent mindComp,
        SoulBoundComponent soulBound)
    {
        base.Resurrect(mind, phylactery, mindComp, soulBound);

        var ent = Spawn(LichPrototype, TransformSystem.GetMapCoordinates(phylactery));
        Mind.TransferTo(mind, ent, mind: mindComp);

        Faction.ClearFactions(ent, false);
        Faction.AddFaction(ent, WizardRuleSystem.Faction);
        RemCompDeferred<TransferMindOnGibComponent>(ent);
        EnsureComp<WizardComponent>(ent);

        SetOutfitCommand.SetOutfit(ent, LichGear, EntityManager);

        if (TryComp(ent, out HumanoidAppearanceComponent? humanoid))
        {
            if (soulBound.Age != null)
                humanoid.Age = soulBound.Age.Value;
            if (soulBound.Gender != null)
                humanoid.Gender = soulBound.Gender.Value;
            if (soulBound.Sex != null)
                humanoid.Sex = soulBound.Sex.Value;
            Dirty(ent, humanoid);
        }

        if (soulBound.Name != string.Empty)
            Meta.SetEntityName(ent, soulBound.Name);

        Stun.TryKnockdown(ent, TimeSpan.FromSeconds(20) + TimeSpan.FromSeconds(5) * soulBound.ResurrectionsCount, true);
        soulBound.ResurrectionsCount++;
        Dirty(mind, soulBound);
    }

    protected override bool RespawnItem(EntityUid item, TransformComponent itemXform, TransformComponent userXform)
    {
        var grid = userXform.GridUid;
        var map = userXform.MapUid;

        if (map == null)
            return false;

        if (grid == null)
        {
            var station = _wizard.GetWizardTargetStation();

            if (station == null)
                return false;

            grid = _station.GetLargestGrid(station.Value.Comp);
            if (grid == null)
                return false;
        }

        if (itemXform.GridUid == grid.Value)
            return true;

        if (!_respawn.TryFindRandomTile(grid.Value, map.Value, 10, out var coords, false))
            return false;

        if (Container.TryGetOuterContainer(item, itemXform, out var container))
            item = container.Owner;

        TransformSystem.SetCoordinates(item, coords);
        return true;
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
