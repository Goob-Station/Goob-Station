using Content.Server.Administration.Systems;
using Content.Server.Antag;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Humanoid;
using Content.Server.Mind.Commands;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Heretic;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.NPC.Systems;
using Content.Shared.Nutrition.AnimalHusbandry;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Audio;

namespace Content.Server.Heretic;

public sealed partial class GhoulSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public void GhoulifyEntity(Entity<GhoulComponent> ent)
    {
        RemComp<RespiratorComponent>(ent);
        RemComp<BarotraumaComponent>(ent);
        RemComp<HungerComponent>(ent);
        RemComp<ThirstComponent>(ent);
        RemComp<ReproductiveComponent>(ent);
        RemComp<ReproductivePartnerComponent>(ent);

        var hasMind = _mind.TryGetMind(ent, out var mindId, out var mind);
        if (hasMind && ent.Comp.BoundHeretic != null)
        {
            var brief = Loc.GetString("heretic-ghoul-greeting", ("ent", Identity.Entity((EntityUid) ent.Comp.BoundHeretic, EntityManager)));
            var sound = new SoundPathSpecifier("/Audio/Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");
            _antag.SendBriefing(ent, brief, Color.MediumPurple, sound);
        }

        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            _humanoid.SetSkinColor(ent, Color.FromHex("#505050"), true, false, humanoid);
            _humanoid.SetBaseLayerColor(ent, HumanoidVisualLayers.Eyes, Color.FromHex("#505050"));
        }

        // todo: add dynamic total damage when it finally stops being private
        _rejuvenate.PerformRejuvenate(ent);

        MakeSentientCommand.MakeSentient(ent, EntityManager);

        if (!HasComp<GhostRoleMobSpawnerComponent>(ent) && !hasMind)
        {
            var ghostRole = EnsureComp<GhostRoleComponent>(ent);
            EnsureComp<GhostTakeoverAvailableComponent>(ent);
            ghostRole.RoleName = "Ghoul";
            ghostRole.RoleDescription = "Serve your master";
            ghostRole.RoleRules = "You'll know it when you see it";
        }

        _faction.ClearFactions((ent, null));
        _faction.AddFaction((ent, null), "Heretic");
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhoulComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<GhoulComponent, AttackAttemptEvent>(OnTryAttack);
        SubscribeLocalEvent<GhoulComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
        SubscribeLocalEvent<GhoulComponent, ExaminedEvent>(OnExamine);
    }

    private void OnInit(Entity<GhoulComponent> ent, ref ComponentInit args)
    {
        GhoulifyEntity(ent);
    }
    private void OnTakeGhostRole(Entity<GhoulComponent> ent, ref TakeGhostRoleEvent args)
    {
        // dirty briefing copypaste
        if (ent.Comp.BoundHeretic != null && args.Player.AttachedEntity != null)
        {
            var brief = Loc.GetString("heretic-ghoul-greeting", ("ent", Name((EntityUid) ent.Comp.BoundHeretic)));
            var sound = new SoundPathSpecifier("/Audio/Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");
            _antag.SendBriefing((EntityUid) args.Player.AttachedEntity, brief, Color.MediumPurple, sound);
        }
    }

    private void OnTryAttack(Entity<GhoulComponent> ent, ref AttackAttemptEvent args)
    {
        // prevent attacking owner and other heretics
        if (args.Target == ent.Owner
        || HasComp<HereticComponent>(args.Target))
            args.Cancel();
    }

    private void OnExamine(Entity<GhoulComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("examine-system-cant-see-entity"));
    }
}
