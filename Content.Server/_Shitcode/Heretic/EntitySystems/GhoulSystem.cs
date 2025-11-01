// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 github-actions <github-actions@github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Systems;
using Content.Server.Antag;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Dragon;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Hands.Systems;
using Content.Server.Humanoid;
using Content.Server.Mind.Commands;
using Content.Server.Roles;
using Content.Server.Storage.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Body.Systems;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Examine;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Heretic;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.AnimalHusbandry;
using Content.Shared.Nutrition.Components;
using Content.Shared.RatKing;
using Robust.Server.Audio;
using Content.Goobstation.Shared.Religion;
using Content.Server.GameTicking.Rules;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared.Gibbing.Events;
using Robust.Shared.Audio;

namespace Content.Server.Heretic.EntitySystems;

public sealed class GhoulSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly StorageSystem _storage = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhoulComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GhoulComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GhoulComponent, AttackAttemptEvent>(OnTryAttack);
        SubscribeLocalEvent<GhoulComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
        SubscribeLocalEvent<GhoulComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<GhoulComponent, MobStateChangedEvent>(OnMobStateChange);

        SubscribeLocalEvent<GhoulWeaponComponent, ExaminedEvent>(OnWeaponExamine);
    }

    private void OnWeaponExamine(Entity<GhoulWeaponComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    public void GhoulifyEntity(Entity<GhoulComponent> ent)
    {
        RemComp<RespiratorComponent>(ent);
        RemComp<BarotraumaComponent>(ent);
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        RemComp<ReproductiveComponent>(ent);
        RemComp<ReproductivePartnerComponent>(ent);
        RemComp<TemperatureComponent>(ent);
        RemComp<ConsciousnessComponent>(ent);
        RemComp<PacifiedComponent>(ent);
        RemComp<XenomorphComponent>(ent);
        RemComp<RatKingComponent>(ent);
        RemComp<DragonComponent>(ent);
        EnsureComp<CombatModeComponent>(ent);

        var hasMind = _mind.TryGetMind(ent, out var mindId, out var mind);
        if (hasMind)
        {
            _mind.UnVisit(mindId, mind);
            SendBriefing(ent, mindId);
        }

        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            // make them "have no eyes" and grey
            // this is clearly a reference to grey tide
            var greycolor = Color.FromHex("#505050");
            _humanoid.SetSkinColor(ent, greycolor, true, false, humanoid);
            _humanoid.SetBaseLayerColor(ent, HumanoidVisualLayers.Eyes, greycolor, true, humanoid);
        }

        _rejuvenate.PerformRejuvenate(ent);
        if (TryComp<MobThresholdsComponent>(ent, out var th))
        {
            _threshold.SetMobStateThreshold(ent, ent.Comp.TotalHealth, MobState.Dead, th);
            _threshold.SetMobStateThreshold(ent, ent.Comp.TotalHealth * 0.99f, MobState.Critical, th);
        }

        MakeSentientCommand.MakeSentient(ent, EntityManager);

        if (!hasMind)
        {
            var ghostRole = EnsureComp<GhostRoleComponent>(ent);
            ghostRole.RoleName = Loc.GetString(ent.Comp.GhostRoleName);
            ghostRole.RoleDescription = Loc.GetString(ent.Comp.GhostRoleDesc);
            ghostRole.RoleRules = Loc.GetString(ent.Comp.GhostRoleRules);
        }

        if (!HasComp<GhostRoleMobSpawnerComponent>(ent) && !hasMind)
            EnsureComp<GhostTakeoverAvailableComponent>(ent);

        _faction.ClearFactions(ent.Owner);
        _faction.AddFaction(ent.Owner, HereticRuleSystem.HereticFactionId);

        if (!ent.Comp.GiveBlade)
            return;

        var blade = Spawn(ent.Comp.BladeProto, Transform(ent).Coordinates);
        EnsureComp<GhoulWeaponComponent>(blade);
        ent.Comp.BoundWeapon = blade;

        if (!_hands.TryPickup(ent, blade, animate: false) &&
            _inventory.TryGetSlotEntity(ent, "back", out var slotEnt) &&
            _storage.CanInsert(slotEnt.Value, blade, out _))
            _storage.Insert(slotEnt.Value, blade, out _, out _, playSound: false);
    }

    private void SendBriefing(Entity<GhoulComponent> ent, EntityUid mindId)
    {
        if (ent.Comp.BoundHeretic == null)
            return;

        var brief = Loc.GetString("heretic-ghoul-greeting-noname");
        var master = ent.Comp.BoundHeretic;

        if (Exists(master))
            brief = Loc.GetString("heretic-ghoul-greeting", ("ent", Identity.Entity(master.Value, EntityManager)));

        var sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");
        _antag.SendBriefing(ent, brief, Color.MediumPurple, sound);

        if (!TryComp<GhoulRoleComponent>(ent, out _))
            AddComp<GhoulRoleComponent>(mindId, new(), overwrite: true);

        if (!TryComp<RoleBriefingComponent>(ent, out var rolebrief))
            AddComp(mindId, new RoleBriefingComponent() { Briefing = brief }, overwrite: true);
        else
            rolebrief.Briefing += $"\n{brief}";
    }

    private void OnStartup(Entity<GhoulComponent> ent, ref ComponentStartup args)
    {
        GhoulifyEntity(ent);
        var unholy = EnsureComp<WeakToHolyComponent>(ent);
        unholy.AlwaysTakeHoly = true; // Shitchap - End
    }

    private void OnShutdown(Entity<GhoulComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.BoundWeapon == null || TerminatingOrDeleted(ent.Comp.BoundWeapon.Value))
            return;

        _audio.PlayPvs(ent.Comp.BladeDeleteSound, Transform(ent.Comp.BoundWeapon.Value).Coordinates);
        QueueDel(ent.Comp.BoundWeapon.Value);
    }

    private void OnTakeGhostRole(Entity<GhoulComponent> ent, ref TakeGhostRoleEvent args)
    {
        var hasMind = _mind.TryGetMind(ent, out var mindId, out _);
        if (hasMind)
            SendBriefing(ent, mindId);
    }

    private void OnTryAttack(Entity<GhoulComponent> ent, ref AttackAttemptEvent args)
    {
        if (args.Target != null && args.Target == ent.Comp.BoundHeretic)
            args.Cancel();
    }

    private void OnExamine(Entity<GhoulComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineMessage == null)
            return;

        args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    private void OnMobStateChange(Entity<GhoulComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (ent.Comp.SpawnOnDeathPrototype != null)
            Spawn(ent.Comp.SpawnOnDeathPrototype.Value, Transform(ent).Coordinates);

        _body.GibBody(ent, contents: ent.Comp.DropOrgansOnDeath ? GibContentsOption.Drop : GibContentsOption.Skip);
    }
}
