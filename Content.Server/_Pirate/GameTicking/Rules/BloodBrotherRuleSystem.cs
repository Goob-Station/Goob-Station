using System.Diagnostics.CodeAnalysis;
using Content.Server._Pirate.GameTicking.Rules.Components;
using Content.Server._Pirate.Objectives.Components;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Popups;
using Content.Server.Preferences.Managers;
using Content.Server.Roles;
using Content.Server.Stunnable;
using Content.Shared._Pirate.BloodBrothers.Components;
using Content.Shared._Pirate.Roles.Components;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Zombies;
using Robust.Server.Player;
using Robust.Shared.Utility;

namespace Content.Server._Pirate.GameTicking.Rules;

public sealed partial class BloodBrotherRuleSystem : GameRuleSystem<BloodBrotherRuleComponent>
{
    [Dependency] private IAdminLogManager _adminLogManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IServerPreferencesManager _preferencesManager = default!;
    [Dependency] private ActionsSystem _actionsSystem = default!;
    [Dependency] private AntagSelectionSystem _antagSystem = default!;
    [Dependency] private MindSystem _mindSystem = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private NpcFactionSystem _npcFactionSystem = default!;
    [Dependency] private ObjectivesSystem _objectivesSystem = default!;
    [Dependency] private PopupSystem _popupSystem = default!;
    [Dependency] private RoleSystem _roleSystem = default!;
    [Dependency] private StunSystem _stunSystem = default!;
    [Dependency] private TargetObjectiveSystem _targetObjectiveSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodBrotherRuleComponent, ObjectivesTextPrependEvent>(OnObjectivesTextPrepend);
        SubscribeLocalEvent<InitialBloodBrotherComponent, BloodBrotherConvertActionEvent>(OnBloodBrotherConvert);
        SubscribeLocalEvent<InitialBloodBrotherComponent, BloodBrotherCheckConvertActionEvent>(OnBloodBrotherCheckConvert);
    }

    private void OnObjectivesTextPrepend(Entity<BloodBrotherRuleComponent> entity, ref ObjectivesTextPrependEvent args)
    {
        var antags = _antagSystem.GetAntagIdentifiers(entity.Owner);

        foreach (var (mind, sessionData, name) in antags)
        {
            if (!_roleSystem.MindHasRole<BloodBrotherRoleComponent>(mind, out var role))
                continue;

            var brotherRole = role.Value.Comp2;

            if (brotherRole.Brother == null)
                continue;

            if (!_mindSystem.TryGetMind(brotherRole.Brother.Value, out _, out var brotherMind)
                || brotherMind.UserId == null)
            {
                var brotherName = TryComp<MetaDataComponent>(brotherRole.Brother.Value, out var brotherMeta)
                    ? brotherMeta.EntityName
                    : Loc.GetString("generic-unknown-title");

                args.Text += "\n" + Loc.GetString("blood-brother-round-end-no-mind",
                    ("name", name),
                    ("username", sessionData.UserName),
                    ("brotherName", brotherName));

                continue;
            }

            var brotherUsername = _playerManager.GetPlayerData(brotherMind.UserId.Value).UserName;

            args.Text += "\n" + Loc.GetString("blood-brother-round-end",
                ("name", name),
                ("username", sessionData.UserName),
                ("brotherName", MetaData(brotherRole.Brother.Value).EntityName),
                ("brotherUsername", (brotherUsername)));
        }
    }

    private void OnBloodBrotherConvert(Entity<InitialBloodBrotherComponent> entity,
        ref BloodBrotherConvertActionEvent args)
    {
        // Check if convertible
        if (!TryComp<BloodBrotherComponent>(entity, out var originalComponent))
            return;

        if (!CanConvert(entity, args.Target, out var failureMessage))
        {
            _popupSystem.PopupEntity(
                Loc.GetString(failureMessage,
                    ("converter", Identity.Entity(entity, _entityManager)),
                    ("converted", Identity.Entity(args.Target, _entityManager))),
                args.Target,
                entity,
                PopupType.MediumCaution);
            return;
        }

        if (!_mindSystem.TryGetMind(entity, out var mindId, out var mind))
            return;

        if (!_mindSystem.TryGetMind(args.Target, out var targetMindId, out var targetMind))
            return;

        var addedTargetRole = false;
        if (!_roleSystem.MindHasRole(targetMindId, out Entity<MindRoleComponent, BloodBrotherRoleComponent>? targetRole))
        {
            _roleSystem.MindAddRole(targetMindId, entity.Comp.BloodBrotherMindRole, targetMind);
            addedTargetRole = true;
            _roleSystem.MindHasRole(targetMindId, out targetRole);
        }

        if (targetRole is not { } bloodBrotherRole)
        {
            if (addedTargetRole)
                _roleSystem.MindRemoveRole<BloodBrotherRoleComponent>(targetMindId);

            Log.Error($"Blood brother role was null after assigning it to {ToPrettyString(args.Target)}.");
            return;
        }

        if (!_objectivesSystem.TryCreateObjective((targetMindId, targetMind),
                entity.Comp.ConvertedBrotherObjective,
                out var newObjective))
        {
            if (addedTargetRole)
                _roleSystem.MindRemoveRole<BloodBrotherRoleComponent>(targetMindId);

            return;
        }

        var targetObjective = EnsureComp<TargetObjectiveComponent>(newObjective.Value);

        _targetObjectiveSystem.SetTarget(newObjective.Value, mindId, targetObjective);
        _targetObjectiveSystem.SetName(newObjective.Value, targetObjective);

        _mindSystem.AddObjective(targetMindId, targetMind, newObjective.Value);

        // Actual conversion logic
        var convertedComp = CopyComp(entity, args.Target, originalComponent);

        _npcFactionSystem.AddFaction(args.Target, entity.Comp.BloodBrotherFaction);

        _adminLogManager.Add(LogType.Mind,
            LogImpact.Medium,
            $"{ToPrettyString(entity)} converted {ToPrettyString(args.Target)} into their Blood Brother");

        originalComponent.Brother = args.Target;
        if (_roleSystem.MindHasRole<BloodBrotherRoleComponent>(mindId, out var role))
        {
            role.Value.Comp2.Brother = args.Target;
            Dirty(role.Value);
        }

        convertedComp.Brother = entity;
        bloodBrotherRole.Comp2.Brother = entity;
        Dirty(bloodBrotherRole);

        foreach (var objective in mind.Objectives)
        {
            if (!HasComp<BloodBrotherTargetComponent>(objective) ||
                !TryComp<TargetObjectiveComponent>(objective, out var brotherTargetObjective))
                continue;

            _targetObjectiveSystem.SetTarget(objective, args.Target, brotherTargetObjective);
            _targetObjectiveSystem.SetName(objective, brotherTargetObjective);
        }

        // Visuals
        _antagSystem.SendBriefing(args.Target,
            Loc.GetString(entity.Comp.BriefingText),
            entity.Comp.BriefingColor,
            entity.Comp.BriefingSound);

        _popupSystem.PopupEntity(
            Loc.GetString(
                entity.Comp.ConvertPopupText,
                ("converter", Identity.Entity(entity, _entityManager)),
                ("converted", Identity.Entity(args.Target, _entityManager))),
            args.Target,
            PopupType.LargeCaution);

        if (entity.Comp.ConvertStunTime != null)
            _stunSystem.TryUpdateParalyzeDuration(args.Target, entity.Comp.ConvertStunTime);

        // Remove the conversion actions
        _actionsSystem.RemoveAction(entity.Comp.ConvertActionEntity);
        _actionsSystem.RemoveAction(entity.Comp.CheckConvertActionEntity);

        // Make sure the components are sent correctly
        Dirty(entity, originalComponent);
        Dirty(args.Target, convertedComp);
    }

    private void OnBloodBrotherCheckConvert(Entity<InitialBloodBrotherComponent> entity,
        ref BloodBrotherCheckConvertActionEvent args)
    {
        if (!CanConvert(entity, args.Target, out var failureMessage))
        {
            _popupSystem.PopupEntity(
                Loc.GetString(failureMessage,
                    ("converter", Identity.Entity(entity, _entityManager)),
                    ("converted", Identity.Entity(args.Target, _entityManager))),
                args.Target,
                entity,
                PopupType.MediumCaution);
            return;
        }

        _popupSystem.PopupEntity(
            Loc.GetString("blood-brother-convert-convertible",
                ("converter", Identity.Entity(entity, _entityManager)),
                ("converted", Identity.Entity(args.Target, _entityManager))),
            args.Target,
            entity,
            PopupType.Medium);
    }

    private bool CanConvert(
        Entity<InitialBloodBrotherComponent> entity,
        EntityUid target,
        [NotNullWhen(false)] out string? errorMessage)
    {
        errorMessage = null;

        if (!_mindSystem.TryGetMind(entity, out _, out var converterMind))
        {
            DebugTools.Assert("Blood brother tried to convert but had no mind.");
            Log.Error("Blood brother tried to convert but had no mind.");
            errorMessage = "blood-brother-convert-failed-no-mind";
            return false; // How would this even happen
        }

        if (!_mindSystem.TryGetMind(target, out var targetMindId, out var targetMind))
        {
            errorMessage = "blood-brother-convert-failed-no-mind";
            return false;
        }

        // Target is already a blood brother
        if (HasComp<BloodBrotherComponent>(target))
        {
            errorMessage = "blood-brother-convert-failed-already-brother";
            return false;
        }

        // Stop the blood brother from converting a target.
        foreach (var objective in converterMind.Objectives)
        {
            if (!TryComp<TargetObjectiveComponent>(objective, out var targetObjective))
                continue;

            if (targetObjective.Target != targetMindId)
                continue;

            errorMessage = "blood-brother-convert-failed-target";
            return false;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            errorMessage = "blood-brother-convert-failed-no-mind";
            return false;
        }

        if (HasComp<ZombieComponent>(target))
        {
            errorMessage = "blood-brother-convert-failed-zombie";
            return false;
        }

        if (targetMind.UserId == null)
        {
            errorMessage = "blood-brother-convert-failed-no-mind";
            return false;
        }

        // Check antag preference
        if (entity.Comp.RequiredAntagPreference != null)
        {
            if (!_preferencesManager.TryGetCachedPreferences(targetMind.UserId.Value, out var preferences))
            {
                errorMessage = "blood-brother-convert-failed-preference";
                return false;
            }

            if (preferences.SelectedCharacter is not HumanoidCharacterProfile profile ||
                !profile.AntagPreferences.Contains(entity.Comp.RequiredAntagPreference!.Value))
            {
                errorMessage = "blood-brother-convert-failed-preference";
                return false;
            }
        }

        if (!_mobStateSystem.IsAlive(target))
        {
            errorMessage = "blood-brother-convert-failed-dead";
            return false;
        }

        if (HasComp<MindShieldComponent>(target))
        {
            errorMessage = "blood-brother-convert-failed-shielded";
            return false;
        }

        return true;
    }
}
