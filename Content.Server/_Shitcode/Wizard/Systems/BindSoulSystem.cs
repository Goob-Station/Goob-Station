// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Commands;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.IdentityManagement;
using Content.Server.Respawn;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class BindSoulSystem : SharedBindSoulSystem
{
    [Dependency] private readonly SpecialRespawnSystem _respawn = default!;
    [Dependency] private readonly WizardRuleSystem _wizard = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

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

        if (soulBound.Name != string.Empty)
            Meta.SetEntityName(ent, soulBound.Name);

        if (TryComp(ent, out HumanoidAppearanceComponent? humanoid))
        {
            if (soulBound.Age != null)
                humanoid.Age = soulBound.Age.Value;
            if (soulBound.Gender != null)
            {
                humanoid.Gender = soulBound.Gender.Value;
                if (TryComp(ent, out GrammarComponent? grammar))
                    Grammar.SetGender((ent, grammar), soulBound.Gender);
                var identity = Identity.Entity(ent, EntityManager);
                if (TryComp(identity, out GrammarComponent? identityGrammar))
                    Grammar.SetGender((identity, identityGrammar), soulBound.Gender);
            }
            if (soulBound.Sex != null)
                humanoid.Sex = soulBound.Sex.Value;
            Dirty(ent, humanoid);
        }

        _identity.QueueIdentityUpdate(ent);

        Stun.TryKnockdown(ent,
            TimeSpan.FromSeconds(20) + TimeSpan.FromSeconds(10) * soulBound.ResurrectionsCount,
            true);
        soulBound.ResurrectionsCount++;
        Dirty(mind, soulBound);
    }

    protected override bool RespawnItem(EntityUid item, TransformComponent itemXform, TransformComponent userXform)
    {
        var grid = userXform.GridUid;
        var map = userXform.MapUid;

        if (map == null)
            return false;

        grid ??= _wizard.GetWizardTargetRandomStationGrid();

        if (grid == null)
            return false;

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