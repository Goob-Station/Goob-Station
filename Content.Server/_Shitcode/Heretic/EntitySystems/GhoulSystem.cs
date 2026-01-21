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

using System.Linq;
using Content.Server.Administration.Systems;
using Content.Server.Antag;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Dragon;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Hands.Components;
using Content.Server.Hands.Systems;
using Content.Server.Humanoid;
using Content.Server.Mind.Commands;
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
using Content.Server.Heretic.Abilities;
using Content.Server.Jittering;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Roles;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Body.Components;
using Content.Shared.Coordinates;
using Content.Shared.Gibbing.Events;
using Content.Shared.Roles;
using Content.Shared.Species.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Polymorph;
using Content.Server.Polymorph.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.NPC.Components;

namespace Content.Server.Heretic.EntitySystems;

public sealed class GhoulSystem : EntitySystem
{
    private static readonly ProtoId<HTNCompoundPrototype> Compound = "HereticSummonCompound";
    private static readonly EntProtoId<MindRoleComponent> GhoulRole = "MindRoleGhoul";

    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly JitteringSystem _jitter = default!;
    [Dependency] private readonly StutteringSystem _stutter = default!;
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
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private readonly List<Type> _componentsToRemoveOnGhoulify =
    [
        typeof(RespiratorComponent),
        typeof(BarotraumaComponent),
        typeof(HungerComponent),
        typeof(ThirstComponent),
        typeof(ReproductiveComponent),
        typeof(ReproductivePartnerComponent),
        typeof(TemperatureComponent),
        typeof(ConsciousnessComponent),
        typeof(PacifiedComponent),
        typeof(XenomorphComponent),
        typeof(RatKingComponent),
        typeof(DragonComponent),
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhoulComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GhoulComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GhoulComponent, AttackAttemptEvent>(OnTryAttack);
        SubscribeLocalEvent<GhoulComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
        SubscribeLocalEvent<GhoulComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<GhoulComponent, MobStateChangedEvent>(OnMobStateChange);

        SubscribeLocalEvent<GhoulDeconvertComponent, ComponentStartup>(OnDeconvertStartup);

        SubscribeLocalEvent<GhoulRoleComponent, GetBriefingEvent>(OnGetBriefing);

        SubscribeLocalEvent<GhoulWeaponComponent, ExaminedEvent>(OnWeaponExamine);

        SubscribeLocalEvent<VoicelessDeadComponent, MapInitEvent>(OnVoicelessDeadInit);
        SubscribeLocalEvent<VoicelessDeadComponent, ComponentShutdown>(OnVoicelessDeadShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GhoulDeconvertComponent, GhoulComponent>();
        while (query.MoveNext(out var uid, out var deconvert, out var ghoul))
        {
            deconvert.Delay -= frameTime;

            if (deconvert.Delay > 0f)
                continue;

            UnGhoulifyEntity((uid, ghoul));
            RemCompDeferred(uid, deconvert);
        }
    }


    private void OnDeconvertStartup(Entity<GhoulDeconvertComponent> ent, ref ComponentStartup args)
    {
        var time = TimeSpan.FromSeconds(ent.Comp.Delay);
        _jitter.DoJitter(ent, time, true);
        _stutter.DoStutter(ent, time, true);
    }

    private void OnVoicelessDeadShutdown(Entity<VoicelessDeadComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        ProcessVoicelessDeadBody(ent, true);
    }

    private void OnVoicelessDeadInit(Entity<VoicelessDeadComponent> ent, ref MapInitEvent args)
    {
        ProcessVoicelessDeadBody(ent, false);
    }

    private void ProcessVoicelessDeadBody(EntityUid uid, bool makeRemovable)
    {
        var woundableQuery = GetEntityQuery<WoundableComponent>();
        foreach (var (partId, partComp) in _body.GetBodyChildren(uid))
        {
            foreach (var (organId, organComp) in _body.GetPartOrgans(partId, partComp))
            {
                organComp.Removable = makeRemovable;
                Dirty(organId, organComp);
            }

            if (!woundableQuery.TryComp(partId, out var woundable))
                continue;

            woundable.CanRemove = makeRemovable;
            Dirty(partId, woundable);
        }
    }

    private void OnGetBriefing(Entity<GhoulRoleComponent> ent, ref GetBriefingEvent args)
    {
        var uid = args.Mind.Comp.OwnedEntity;

        if (!TryComp(uid, out GhoulComponent? ghoul))
            return;

        var start = Loc.GetString("heretic-ghoul-briefing-start-noname");
        var master = ghoul.BoundHeretic;

        if (Exists(master))
        {
            start = Loc.GetString("heretic-ghoul-briefing-start",
                ("ent", Identity.Entity(master.Value, EntityManager)));
        }

        args.Append(start);
        args.Append(Loc.GetString("heretic-ghoul-briefing-end"));
    }

    private void OnWeaponExamine(Entity<GhoulWeaponComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    public void SetBoundHeretic(Entity<GhoulComponent> ent, EntityUid heretic, bool dirty = true)
    {
        ent.Comp.BoundHeretic = heretic;
        _npc.SetBlackboard(ent, NPCBlackboard.FollowTarget, heretic.ToCoordinates());
        if (dirty)
            Dirty(ent);
    }

    public void UnGhoulifyEntity(Entity<GhoulComponent> ent)
    {
        if (!ent.Comp.CanDeconvert)
            return;

        if (!TryComp(ent, out HumanoidAppearanceComponent? humanoid))
        {
            if (Prototype(ent) is not { } proto)
                return;

            var config = new PolymorphConfiguration
            {
                Entity = proto,
                TransferDamage = true,
                TransferName = true,
                Forced = true,
                RevertOnCrit = false,
                RevertOnDeath = false,
                RevertOnEat = false,
                AllowRepeatedMorphs = true,
            };

            _polymorph.PolymorphEntity(ent, config);
            return;
        }

        _humanoid.SetSkinColor(ent, ent.Comp.OldSkinColor, true, false, humanoid);
        _humanoid.SetBaseLayerColor(ent, HumanoidVisualLayers.Eyes, ent.Comp.OldEyeColor, true, humanoid);

        var species = _proto.Index(humanoid.Species);
        var prototype = _proto.Index(species.Prototype);
        foreach (var comp in _componentsToRemoveOnGhoulify.Concat([typeof(MobThresholdsComponent)]))
        {
            var name = _compFact.GetComponentName(comp);
            if (!prototype.Components.TryGetValue(name, out var reg))
                continue;

            var newComp = _compFact.GetComponent(reg);
            AddComp(ent, newComp, true);
        }

        if (TryComp(ent, out CollectiveMindComponent? collective))
            collective.Channels.Remove(HereticAbilitySystem.MansusLinkMind);

        if (TryComp(ent, out NpcFactionMemberComponent? fact))
        {
            _faction.ClearFactions((ent, fact));
            _faction.AddFactions((ent.Owner, fact), ent.Comp.OldFactions);
        }

        if (_mind.TryGetMind(ent, out var mindId, out var mind))
            _role.MindRemoveRole<GhoulComponent>((mindId, mind));

        if (Exists(ent.Comp.BoundHeretic) && TryComp(ent.Comp.BoundHeretic.Value, out HereticComponent? heretic))
        {
            foreach (var (_, list) in heretic.LimitedTransmutations)
            {
                list.Remove(ent);
            }
        }

        RemComp<GhostTakeoverAvailableComponent>(ent);
        RemComp<GhoulComponent>(ent);
        RemComp<VoicelessDeadComponent>(ent);
        RemComp<HereticBladeUserBonusDamageComponent>(ent);
    }

    public void GhoulifyEntity(Entity<GhoulComponent> ent)
    {
        foreach (var component in _componentsToRemoveOnGhoulify)
        {
            RemComp(ent, component);
        }

        EnsureComp<CombatModeComponent>(ent);

        EnsureComp<CollectiveMindComponent>(ent).Channels.Add(HereticAbilitySystem.MansusLinkMind);

        if (Exists(ent.Comp.BoundHeretic))
            SetBoundHeretic(ent, ent.Comp.BoundHeretic.Value, false);

        if (TryComp(ent.Owner, out NpcFactionMemberComponent? fact))
        {
            ent.Comp.OldFactions = fact.Factions.ToHashSet();

            _faction.ClearFactions((ent.Owner, fact));
            _faction.AddFaction((ent.Owner, fact), HereticRuleSystem.HereticFactionId);
        }

        var hasMind = _mind.TryGetMind(ent, out var mindId, out var mind);
        if (hasMind)
        {
            _mind.UnVisit(mindId, mind);
            if (!_role.MindHasRole<GhoulRoleComponent>(mindId))
            {
                SendBriefing(ent);
                _role.MindAddRole(mindId, GhoulRole, mind);
            }
        }
        else
        {
            var htn = EnsureComp<HTNComponent>(ent);
            htn.RootTask = new HTNCompoundTask { Task = Compound };
            _htn.Replan(htn);
        }

        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            ent.Comp.OldSkinColor = humanoid.SkinColor;
            ent.Comp.OldEyeColor = humanoid.EyeColor;

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
            ghostRole.MindRoles = [GhoulRole];
        }

        if (!HasComp<GhostRoleMobSpawnerComponent>(ent) && !hasMind)
            EnsureComp<GhostTakeoverAvailableComponent>(ent);

        if (TryComp(ent, out FleshMimickedComponent? mimicked))
        {
            foreach (var mimic in mimicked.FleshMimics)
            {
                if (!Exists(mimic))
                    continue;

                _faction.DeAggroEntity(mimic, ent);
            }

            RemCompDeferred(ent, mimicked);
        }

        GiveGhoulWeapon(ent);
    }

    private void SendBriefing(Entity<GhoulComponent> ent)
    {
        var brief = Loc.GetString("heretic-ghoul-greeting-noname");
        var master = ent.Comp.BoundHeretic;

        if (Exists(master))
            brief = Loc.GetString("heretic-ghoul-greeting", ("ent", Identity.Entity(master.Value, EntityManager)));

        var sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");
        _antag.SendBriefing(ent, brief, Color.MediumPurple, sound);
    }

    private void OnStartup(Entity<GhoulComponent> ent, ref ComponentStartup args)
    {
        GhoulifyEntity(ent);
        var unholy = EnsureComp<WeakToHolyComponent>(ent);
        unholy.AlwaysTakeHoly = true;
    }

    private void OnShutdown(Entity<GhoulComponent> ent, ref ComponentShutdown args)
    {
        DestroyGhoulWeapon(ent);
    }

    private void OnTakeGhostRole(Entity<GhoulComponent> ent, ref TakeGhostRoleEvent args)
    {
        SendBriefing(ent);
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

    private void GiveGhoulWeapon(Entity<GhoulComponent> ent)
    {
        if (!ent.Comp.GiveBlade || !TryComp(ent, out HandsComponent? hands) || Exists(ent.Comp.BoundWeapon))
            return;

        var blade = Spawn(ent.Comp.BladeProto, Transform(ent).Coordinates);
        EnsureComp<GhoulWeaponComponent>(blade);
        ent.Comp.BoundWeapon = blade;

        if (!_hands.TryPickup(ent, blade, animate: false, handsComp: hands) &&
            _inventory.TryGetSlotEntity(ent, "back", out var slotEnt) &&
            _storage.CanInsert(slotEnt.Value, blade, out _))
            _storage.Insert(slotEnt.Value, blade, out _, out _, playSound: false);
    }

    private void DestroyGhoulWeapon(Entity<GhoulComponent> ent)
    {
        if (ent.Comp.BoundWeapon == null || TerminatingOrDeleted(ent.Comp.BoundWeapon.Value))
            return;

        _audio.PlayPvs(ent.Comp.BladeDeleteSound, Transform(ent.Comp.BoundWeapon.Value).Coordinates);
        QueueDel(ent.Comp.BoundWeapon.Value);
    }

    private void OnMobStateChange(Entity<GhoulComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
        {
            if (args.NewMobState == MobState.Alive)
                GiveGhoulWeapon(ent);
            return;
        }

        DestroyGhoulWeapon(ent);

        if (ent.Comp.DeathBehavior == GhoulDeathBehavior.NoGib)
            return;

        if (ent.Comp.SpawnOnDeathPrototype != null)
            Spawn(ent.Comp.SpawnOnDeathPrototype.Value, Transform(ent).Coordinates);

        if (!TryComp(ent, out BodyComponent? body))
            return;

        foreach (var nymph in _body.GetBodyOrganEntityComps<NymphComponent>((ent, body)))
        {
            RemComp(nymph.Owner, nymph.Comp1);
        }

        _body.GibBody(ent,
            body: body,
            contents: ent.Comp.DeathBehavior == GhoulDeathBehavior.GibOrgans
                ? GibContentsOption.Drop
                : GibContentsOption.Skip);
    }
}
