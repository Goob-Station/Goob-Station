// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Text;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.PartStatus.Events;
using Content.Shared.Body.Part;
using Content.Shared.Chat;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Shitmed.PartStatus;
public sealed class PartStatusSystem : EntitySystem
{
    [Dependency] private readonly WoundSystem _woundSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    private static readonly List<string> _bodyPartOrder = new()
    {
        "head",
        "chest",
        "left arm",
        "left hand",
        "right arm",
        "left leg",
        "left foot",
        "right leg",
        "right foot"
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GetPartStatusEvent>(OnGetPartStatus);
    }

    private void OnGetPartStatus(GetPartStatusEvent message, EntitySessionEventArgs args)
    {
        var entity = GetEntity(message.Uid);
        if (_mobStateSystem.IsIncapacitated(entity))
            return;

        if (!TryComp<ActorComponent>(entity, out var actor))
            return;

        if (_bodySystem.GetRootPartOrNull(entity) is not { } rootPart)
            return;

        HashSet<PartStatus> partStatusSet = [];
        foreach (var woundable in _woundSystem.GetAllWoundableChildren(rootPart.Entity)) // honestly nuke part status at somepoint
        {
            // TODO: ADD ORGAN and TRAUMA STATUS. For chest and head. headaches migraines, sharp pains, tightness in chest.

            if (!TryComp<BodyPartComponent>(woundable, out var bodyPartComponent)
                || !TryComp<BoneComponent>(woundable.Comp.Bone.ContainedEntities.FirstOrNull(), out var bone))
                continue;

            var isBleeding = false;
            var boneSeverity = bone.BoneSeverity;
            var name = bodyPartComponent.ParentSlot is null ? bodyPartComponent.PartType.ToString().ToLower() : bodyPartComponent.ParentSlot.Value.Id;

            var damageSeverities = new Dictionary<string, WoundSeverity>();

            foreach (var wound in _woundSystem.GetWoundableWounds(woundable))
            {
                if (wound.Comp.DamageGroup == null)
                    return;

                if (!damageSeverities.TryGetValue(wound.Comp.DamageType, out var existingSeverity) ||
                    wound.Comp.WoundSeverity > existingSeverity)
                    damageSeverities[wound.Comp.DamageGroup.LocalizedName] = wound.Comp.WoundSeverity;

                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    continue;

                if(bleeds.IsBleeding)
                    isBleeding = true;
                Logger.Debug($"Added status: {name} Bleeding={isBleeding}");
            }

            partStatusSet.Add(new PartStatus(
                name,
                woundable.Comp.WoundableSeverity,
                damageSeverities,
                boneSeverity,
                isBleeding));
        }

        var text = GetExamineText(entity, entity, partStatusSet);
        _chat.ChatMessageToOne(ChatChannel.Emotes, text.ToString(), text.ToString(), EntityUid.Invalid, false, actor.PlayerSession.Channel, recordReplay: false);
    }

    private FormattedMessage GetExamineText(EntityUid entity, EntityUid examiner, HashSet<PartStatus> partStatusSet)
    {
        var inspectingSelf = entity == examiner;
        var message = new FormattedMessage();
        message.PushColor(Color.DarkGray);

        CreateBodyPartMessage(partStatusSet, inspectingSelf, ref message);

        var examinedEvent = new PartStatusExaminedEvent(message,
            examiner,
            entity);

        RaiseLocalEvent(entity, examinedEvent);

        var newMessage = examinedEvent.GetTotalMessage();

        // Goobstation Change: I dont seem to have a way to get the event of examination to happen after EVERYTHING else, so fuck it.
        //var examineCompletedEvent = new ExamineCompletedEvent(newMessage, entity, examiner);
        //RaiseLocalEvent(entity, examineCompletedEvent);
        // pop color tag
        newMessage.Pop();

        return newMessage;
    }

    private void CreateBodyPartMessage(HashSet<PartStatus> partStatusSet, bool inspectingSelf, ref FormattedMessage message)
    {
        foreach (var partStatus in _bodyPartOrder.Select(bodyPart => partStatusSet.FirstOrDefault(t => t.PartType == bodyPart)).OfType<PartStatus>())
        {
            const string bleedLocaleStr = "inspect-wound-Bleeding-moderate";
            const string boneLocaleStr = "inspect-trauma-BoneDamage";

            var sb = new StringBuilder();
            var hasStatus = false;

            if (partStatus.Bleeding)
            {
                sb.Append($"{Loc.GetString(inspectingSelf ? "self-" + bleedLocaleStr : bleedLocaleStr)}");
                sb.Append(partStatus.BoneSeverity > BoneSeverity.Normal ? ", " : " ");
                hasStatus = true;
            }

            if (partStatus.BoneSeverity > BoneSeverity.Normal)
            {
                sb.Append($"{Loc.GetString(inspectingSelf ? "self-" + boneLocaleStr : boneLocaleStr)}, ");
                hasStatus = true;
            }

            foreach (var (type, severity) in partStatus.DamageSeverities)
            {
                if (type is not ("Brute" or "Burn"))
                    continue;

                var newSev = severity > WoundSeverity.Severe ? WoundSeverity.Severe : severity;
                var localeText = $"inspect-wound-{type}-{newSev.ToString().ToLower()}";
                var text = $"{Loc.GetString(inspectingSelf ? "self-" + localeText : localeText)}";
                if (hasStatus && !text.Contains("and", StringComparison.CurrentCultureIgnoreCase))
                    sb.Append("and ");
                sb.Append(text);
                hasStatus = true;
            }

            if (!hasStatus)
                sb.Append("fine");

            message.AddMarkupOrThrow($"[font size=10]{(inspectingSelf ? "Your" : "Their")} [bold]{partStatus.PartType}[/bold] is {sb}.[/font]");
            message.PushNewline();
        }
    }
}
