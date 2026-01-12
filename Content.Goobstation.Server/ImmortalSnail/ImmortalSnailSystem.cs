using Content.Goobstation.Shared.ImmortalSnail;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Mind;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.ImmortalSnail;

public sealed class ImmortalSnailSystem : SharedImmortalSnailSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    protected override EntityUid? GetOwningStation(EntityUid entity)
    {
        return _station.GetOwningStation(entity);
    }

    protected override void OnTouchOfDeathBuffs(EntityUid snail, string targetName)
    {
        // Get the rule component to pass along
        ImmortalSnailRuleComponent? ruleComp = null;
        if (TryComp<ImmortalSnailComponent>(snail, out var snailComp)
            && snailComp.RuleEntity != null
            && TryComp<ImmortalSnailRuleComponent>(snailComp.RuleEntity.Value, out var comp))
            ruleComp = comp;

        GiveSnailBuffs(snail, targetName, ruleComp, playAnnouncement: true);
    }

    /// <summary>
    /// Gives the snail wizard buffs and speed increase after successful kill.
    /// </summary>
    /// <param name="snail">The snail entity</param>
    /// <param name="targetName">The name of the killed target</param>
    /// <param name="ruleComp">The rule component for sound configuration</param>
    /// <param name="playAnnouncement">Whether to play a global announcement</param>
    public void GiveSnailBuffs(EntityUid snail, string targetName, ImmortalSnailRuleComponent? ruleComp = null, bool playAnnouncement = true)
    {
        EnsureComp<Content.Shared._Goobstation.Wizard.WizardComponent>(snail);

        if (TryComp<MovementSpeedModifierComponent>(snail, out var moveSpeed))
        {
            var newWalkSpeed = moveSpeed.BaseWalkSpeed * 2.5f;
            var newSprintSpeed = moveSpeed.BaseSprintSpeed * 2.5f;
            var acceleration = moveSpeed.BaseAcceleration;
            _movementSpeed.ChangeBaseSpeed(snail, newWalkSpeed, newSprintSpeed, acceleration, moveSpeed);
        }

        if (!_mind.TryGetMind(snail, out var mindId, out _))
            return;

        var container = EnsureComp<ActionsContainerComponent>(mindId);

        var spellsToGrant = new List<EntProtoId>
        {
            "ActionBananaTouchNoRobes",
        };

        foreach (var spell in spellsToGrant)
            _actionContainer.AddAction(mindId, spell, container);

        if (playAnnouncement && ruleComp != null && ruleComp.PlayAnnouncements)
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("immortal-snail-becomes-wizard", ("target", targetName)),
                sender: Loc.GetString("immortal-snail-announcement-sender"),
                announcementSound: ruleComp.HonkSound,
                colorOverride: Color.Yellow);
    }
}
