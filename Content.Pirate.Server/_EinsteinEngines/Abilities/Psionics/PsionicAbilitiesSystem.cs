using Content.Server.Chat.Managers;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.NPC.HTN;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Popups;
using Content.Shared.Chat;
using Content.Shared.Psionics;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffect;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Player;
using Content.Pirate.Common.CCVar;
using Content.Shared.NPC.Systems;

namespace Content.Server.Abilities.Psionics;

public sealed class PsionicAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly PsionicFamiliarSystem _psionicFamiliar = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InnatePsionicPowersComponent, MapInitEvent>(InnatePowerStartup);
        SubscribeLocalEvent<PsionicComponent, ComponentShutdown>(OnPsionicShutdown);
    }

    /// <summary>
    ///     Special use-case for a InnatePsionicPowers, which allows an entity to start with any number of Psionic Powers.
    /// </summary>
    private void InnatePowerStartup(EntityUid uid, InnatePsionicPowersComponent comp, MapInitEvent args)
    {
        // Any entity with InnatePowers should also be psionic, but in case they aren't already...
        EnsureComp<PsionicComponent>(uid, out var psionic);

        foreach (var proto in comp.PowersToAdd)
            if (!psionic.ActivePowers.Contains(_prototypeManager.Index(proto)))
                InitializePsionicPower(uid, _prototypeManager.Index(proto), psionic, false);
    }

    private void OnPsionicShutdown(EntityUid uid, PsionicComponent component, ComponentShutdown args)
    {
        if (!EntityManager.EntityExists(uid)
            || HasComp<MindbrokenComponent>(uid))
            return;

        KillFamiliars(component);
        RemoveAllPsionicPowers(uid);
    }

    /// <summary>
    ///     The most shorthand route to creating a Psion. If an entity is not already psionic, it becomes one. This also adds a random new PsionicPower.
    ///     To create a "Latent Psychic"(Psion with no powers) just add or ensure the PsionicComponent normally.
    /// </summary>
    public void AddPsionics(EntityUid uid)
    {
        if (Deleted(uid))
            return;

        AddRandomPsionicPower(uid);
    }

    /// <summary>
    ///     Pretty straightforward, adds a random psionic power to a given Entity. If that Entity is not already Psychic, it will be made one.
    ///     If an entity already has all possible powers, this will not add any new ones.
    /// </summary>
    public void AddRandomPsionicPower(EntityUid uid, bool forced = false)
    {
        // We need to EnsureComp here to make sure that we aren't iterating over a component that:
        // A: Isn't fully initialized
        // B: Is in the process of being shutdown/deleted
        // Imagine my surprise when I found out Resolve doesn't check for that.
        // TODO: This EnsureComp will be 1984'd in a separate PR, when I rework how you get psionics in the first place.
        EnsureComp<PsionicComponent>(uid, out var psionic);
        if (!psionic.Roller && !forced)
            return;

        // Since this can be called by systems other than the original roundstart initialization, we need to check that the available powers list
        // doesn't contain duplicates of powers we already have.
        var copy = _serialization.CreateCopy(psionic.AvailablePowers, notNullableOverride: true);
        foreach (var weight in copy)
        {
            if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(weight.Key, out var copyPower)
                || !psionic.ActivePowers.Contains(copyPower))
                continue;

            psionic.AvailablePowers.Remove(copyPower.ID);
        }

        if (psionic.AvailablePowers.Count <= 0)
            return;
        var proto = _random.Pick(psionic.AvailablePowers);
        if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(proto, out var newPower))
            return;

        InitializePsionicPower(uid, newPower);
    }

    /// <summary>
    ///     Initializes a new Psionic Power on a given entity, assuming the entity does not already have said power initialized.
    /// </summary>
    public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, PsionicComponent psionic, bool playFeedback = true)
    {
        if (!_prototypeManager.HasIndex<PsionicPowerPrototype>(proto.ID)
            || psionic.ActivePowers.Contains(proto))
            return;

        psionic.ActivePowers.Add(proto);

        foreach (var function in proto.InitializeFunctions)
            function.OnAddPsionic(uid,
                _componentFactory,
                EntityManager,
                _serialization,
                _playerManager,
                Loc,
                psionic,
                proto);

        RefreshPsionicModifiers(uid, psionic);
        UpdatePowerSlots(psionic);
    }

    /// <summary>
    ///     Initializes a new Psionic Power on a given entity, assuming the entity does not already have said power initialized.
    /// </summary>
    public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, bool playFeedback = true)
    {
        EnsureComp<PsionicComponent>(uid, out var psionic);

        InitializePsionicPower(uid, proto, psionic, playFeedback);
    }

    /// <summary>
    ///     Updates a Psion's casting stats, call this anytime a system adds a new source of Amp or Damp.
    /// </summary>
    public void RefreshPsionicModifiers(EntityUid uid, PsionicComponent comp)
    {
        var ampModifier = 0f;
        var dampModifier = 0f;
        foreach (var (_, source) in comp.AmplificationSources)
            ampModifier += source;
        foreach (var (_, source) in comp.DampeningSources)
            dampModifier += source;

        var ev = new OnSetPsionicStatsEvent(ampModifier, dampModifier);
        RaiseLocalEvent(uid, ref ev);
        ampModifier = ev.AmplificationChangedAmount;
        dampModifier = ev.DampeningChangedAmount;

        comp.CurrentAmplification = ampModifier;
        comp.CurrentDampening = dampModifier;
        Dirty(uid, comp);
    }

    /// <summary>
    ///     Updates a Psion's casting stats, call this anytime a system adds a new source of Amp or Damp.
    ///     Variant function for systems that didn't already have the PsionicComponent.
    /// </summary>
    public void RefreshPsionicModifiers(EntityUid uid)
    {
        if (!TryComp<PsionicComponent>(uid, out var comp))
            return;

        RefreshPsionicModifiers(uid, comp);
    }

    /// <summary>
    ///     A more advanced form of removing powers. Mindbreaking not only removes all psionic powers,
    ///     it also disables the possibility of obtaining new ones.
    /// </summary>
    public void MindBreak(EntityUid uid)
    {
        if (!TryComp<PsionicComponent>(uid, out var psionic))
            return;

        RemoveAllPsionicPowers(uid, true);
        EnsureComp<MindbrokenComponent>(uid);
        _statusEffectsSystem.TryAddStatusEffect(uid, psionic.MindbreakingStutterCondition,
            TimeSpan.FromMinutes(psionic.MindbreakingStutterTime * psionic.CurrentAmplification * psionic.CurrentDampening),
            false,
            psionic.MindbreakingStutterAccent);

        _popups.PopupEntity(Loc.GetString(psionic.MindbreakingFeedback, ("entity", MetaData(uid).EntityName)), uid, uid, PopupType.MediumCaution);

        KillFamiliars(psionic);
        RemComp<PsionicComponent>(uid);
        RemComp<InnatePsionicPowersComponent>(uid);

        var ev = new OnMindbreakEvent();
        RaiseLocalEvent(uid, ref ev);

        if (_config.GetCVar(CCVars.ScarierMindbreaking))
            ScarierMindbreak(uid, psionic);
    }

    /// <summary>
    ///     An even more advanced form of Mindbreaking. Turn the victim into an NPC.
    ///     For the people who somehow didn't intuit from the absolutely horrifying text that mindbreaking people is very fucking bad.
    /// </summary>
    public void ScarierMindbreak(EntityUid uid, PsionicComponent component)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session) || session is null)
            return;

        var popup = Loc.GetString(component.HardMindbreakingFeedback);
        var feedbackMessage = $"[font size=24][color=#ff0000]{popup}[/color][/font]";
        _chatManager.ChatMessageToOne(
            ChatChannel.Emotes,
            feedbackMessage,
            feedbackMessage,
            EntityUid.Invalid,
            false,
            session.Channel);

        if (!_mind.TryGetMind(session, out var mindId, out var mind))
            return;

        _ghost.SpawnGhost((mindId, mind), Transform(uid).Coordinates, false);
        _npcFaction.AddFaction(uid, "SimpleNeutral");
        var htn = EnsureComp<HTNComponent>(uid);
        htn.RootTask = new HTNCompoundTask() { Task = "IdleCompound" };
    }

    /// <summary>
    ///     Remove all Psionic powers, with accompanying actions, components, and casting stat sources, from a given Psion.
    ///     Optionally, the Psion can also be rendered permanently non-Psionic.
    /// </summary>
    public void RemoveAllPsionicPowers(EntityUid uid, bool mindbreak = false)
    {
        if (!TryComp<PsionicComponent>(uid, out var psionic)
            || !psionic.Removable)
            return;

        foreach (var proto in psionic.ActivePowers)
            RemovePsionicPower(uid, psionic, proto, mindbreak);

        if (mindbreak)
            return;

        RefreshPsionicModifiers(uid, psionic);
    }

    public void RemovePsionicPower(EntityUid uid, PsionicComponent psionicComponent, PsionicPowerPrototype psionicPower, bool forced = false)
    {
        if (!psionicComponent.ActivePowers.Contains(psionicPower)
            || !psionicComponent.Removable && !forced)
            return;

        foreach (var function in psionicPower.RemovalFunctions)
            function.OnAddPsionic(uid,
                _componentFactory,
                EntityManager,
                _serialization,
                _playerManager,
                Loc,
                psionicComponent,
                psionicPower);
    }

    public void RemovePsionicPower(EntityUid uid, PsionicPowerPrototype psionicPower, bool forced = false)
    {
        if (!TryComp<PsionicComponent>(uid, out var psionicComponent)
            || !psionicComponent.ActivePowers.Contains(psionicPower)
            || !psionicComponent.Removable && !forced)
            return;

        foreach (var function in psionicPower.RemovalFunctions)
            function.OnAddPsionic(uid,
                _componentFactory,
                EntityManager,
                _serialization,
                _playerManager,
                Loc,
                psionicComponent,
                psionicPower);
    }

    private void UpdatePowerSlots(PsionicComponent psionic)
    {
        var slotsUsed = 0;
        foreach (var power in psionic.ActivePowers)
            slotsUsed += power.PowerSlotCost;

        psionic.PowerSlotsTaken = slotsUsed;
    }

    private void KillFamiliars(PsionicComponent component)
    {
        if (component.Familiars.Count <= 0)
            return;

        foreach (var familiar in component.Familiars)
        {
            if (!TryComp<PsionicFamiliarComponent>(familiar, out var familiarComponent)
                || !familiarComponent.DespawnOnMasterDeath)
                continue;

            _psionicFamiliar.DespawnFamiliar(familiar, familiarComponent);
        }
    }
}
