using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Robust.Shared.Map;
using System.Numerics;
using Content.Shared.NPC.Components;
using NpcFactionSystem = Content.Shared.NPC.Systems.NpcFactionSystem;


namespace Content.Server.Abilities.Psionics;

public sealed partial class PsionicFamiliarSystem : EntitySystem
{
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly NpcFactionSystem _factions = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!; // PIRATE

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, SummonPsionicFamiliarActionEvent>(OnSummon);
        SubscribeLocalEvent<PsionicFamiliarComponent, ComponentShutdown>(OnFamiliarShutdown);
        SubscribeLocalEvent<PsionicFamiliarComponent, AttackAttemptEvent>(OnFamiliarAttack);
        SubscribeLocalEvent<PsionicFamiliarComponent, MobStateChangedEvent>(OnFamiliarDeath);
    }

    private void OnSummon(EntityUid uid, PsionicComponent psionicComponent, SummonPsionicFamiliarActionEvent args)
    {
        // PIRATE - TELEPORT FAMILIARS TO PLAYER
        var playerCoords = Transform(uid).Coordinates;

        if (psionicComponent.Familiars.Count > 0)
        {
            foreach (var familiarUid in psionicComponent.Familiars)
            {
                _transformSystem.SetCoordinates(familiarUid, playerCoords);
                _transformSystem.AttachToGridOrMap(familiarUid, Transform(familiarUid));
            }
        }
        // END PIRATE

        if (psionicComponent.Familiars.Count >= psionicComponent.FamiliarLimit
            || !_psionics.OnAttemptPowerUse(args.Performer, args.PowerName, args.CheckInsulation)
            || args.Handled || args.FamiliarProto is null)
            return;

        args.Handled = true;
        var familiar = Spawn(args.FamiliarProto, Transform(uid).Coordinates);
        EnsureComp<PsionicFamiliarComponent>(familiar, out var familiarComponent);
        familiarComponent.Master = uid;
        psionicComponent.Familiars.Add(familiar);
        Dirty(familiar, familiarComponent);
        Dirty(uid, psionicComponent);

        InheritFactions(uid, familiar, familiarComponent);
        HandleBlackboards(uid, familiar, args);
        DoGlimmerEffects(uid, psionicComponent, args);
    }

    private void InheritFactions(EntityUid uid, EntityUid familiar, PsionicFamiliarComponent familiarComponent)
    {
        if (!familiarComponent.InheritMasterFactions
            || !TryComp<NpcFactionMemberComponent>(uid, out var masterFactions)
            || masterFactions.Factions.Count <= 0)
            return;

        EnsureComp<NpcFactionMemberComponent>(familiar, out var familiarFactions);
        foreach (var faction in masterFactions.Factions)
        {
            if (_factions.IsMember(familiar, faction))
                continue;

            _factions.AddFaction(familiar, faction, true);
        }
    }

    private void HandleBlackboards(EntityUid master, EntityUid familiar, SummonPsionicFamiliarActionEvent args)
    {
        if (!args.FollowMaster
            || !TryComp<HTNComponent>(familiar, out var htnComponent))
            return;

        _npc.SetBlackboard(familiar, NPCBlackboard.FollowTarget, new EntityCoordinates(master, Vector2.Zero), htnComponent);
        _htn.Replan(htnComponent);
    }

    private void DoGlimmerEffects(EntityUid uid, PsionicComponent component, SummonPsionicFamiliarActionEvent args)
    {
        if (!args.DoGlimmerEffects
            || args.MinGlimmer == 0 && args.MaxGlimmer == 0)
            return;

        var minGlimmer = (int) Math.Round(MathF.MinMagnitude(args.MinGlimmer, args.MaxGlimmer)
            * component.CurrentAmplification - component.CurrentDampening);
        var maxGlimmer = (int) Math.Round(MathF.MaxMagnitude(args.MinGlimmer, args.MaxGlimmer)
            * component.CurrentAmplification - component.CurrentDampening);

        _psionics.LogPowerUsed(uid, args.PowerName, minGlimmer, maxGlimmer);
    }

    private void OnFamiliarShutdown(EntityUid uid, PsionicFamiliarComponent component, ComponentShutdown args)
    {
        if (!Exists(component.Master)
            || !TryComp<PsionicComponent>(component.Master, out var psionicComponent)
            || !psionicComponent.Familiars.Contains(uid))
            return;

        psionicComponent.Familiars.Remove(uid);
    }

    private void OnFamiliarAttack(EntityUid uid, PsionicFamiliarComponent component, AttackAttemptEvent args)
    {
        if (component.CanAttackMaster || args.Target is null
            || args.Target != component.Master)
            return;

        args.Cancel();
        if (!Loc.TryGetString(component.AttackMasterText, out var attackFailMessage))
            return;

        _popup.PopupEntity(attackFailMessage, uid, uid, component.AttackPopupType);
    }

    private void OnFamiliarDeath(EntityUid uid, PsionicFamiliarComponent component, MobStateChangedEvent args)
    {
        if (!component.DespawnOnFamiliarDeath
            || args.NewMobState != MobState.Dead)
            return;

        DespawnFamiliar(uid, component);
    }

    public void DespawnFamiliar(EntityUid uid)
    {
        if (!TryComp<PsionicFamiliarComponent>(uid, out var familiarComponent))
            return;

        DespawnFamiliar(uid, familiarComponent);
    }

    public void DespawnFamiliar(EntityUid uid, PsionicFamiliarComponent component)
    {
        var popupText = Loc.GetString(component.DespawnText, ("entity", MetaData(uid).EntityName));
        _popup.PopupEntity(popupText, uid, component.DespawnPopopType);
        QueueDel(uid);
    }
}
