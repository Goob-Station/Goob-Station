// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Server.IdentityManagement;
using Content.Goobstation.Common.Examine; // Goobstation Change
using Content.Goobstation.Common.CCVar; // Goobstation Change
using Content.Shared._Goobstation.Heretic.Components; // Goobstation Change
using Content.Shared.Chat;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Globalization;

namespace Content.Server._White.Examine;
public sealed class ExaminableCharacterSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IdentitySystem _identitySystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly INetConfigurationManager _netConfigManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExaminableCharacterComponent, ExaminedEvent>(HandleExamine);
        SubscribeLocalEvent<MetaDataComponent, ExamineCompletedEvent>(HandleExamine);
    }

    private void HandleExamine(EntityUid uid, ExaminableCharacterComponent comp, ExaminedEvent args)
    {
        if (!TryComp<ActorComponent>(args.Examiner, out var actorComponent)
            || !args.IsInDetailsRange)
            return;

        var showExamine = _netConfigManager.GetClientCVar(actorComponent.PlayerSession.Channel, GoobCVars.DetailedExamine);

        var selfaware = args.Examiner == args.Examined;
        var logLines = new List<string>();

        string canseeloc = "examine-can-see";
        string nameloc = "examine-name";

        if (selfaware)
        {
            canseeloc += "-selfaware";
            nameloc += "-selfaware";
        }
        var identity = _identitySystem.GetEntityIdentity(uid);
        var name = Loc.GetString(nameloc, ("name", identity));
        var cansee = Loc.GetString(canseeloc, ("ent", uid));
        logLines.Add($"[color=DarkGray][font size=10]{cansee}[/font][/color]");

        var slotLabels = new Dictionary<string, string>
        {
            { "head", "head-" },
            { "eyes", "eyes-" },
            { "mask", "mask-" },
            { "neck", "neck-" },
            { "ears", "ears-" },
            { "jumpsuit", "jumpsuit-" },
            { "outerClothing", "outer-" },
            { "back", "back-" },
            { "gloves", "gloves-" },
            { "belt", "belt-" },
            { "id", "id-" },
            { "shoes", "shoes-" },
            { "suitstorage", "suitstorage-" }
        };

        var priority = 13;

        foreach (var slotEntry in slotLabels)
        {
            var slotName = slotEntry.Key;
            var slotLabel = slotEntry.Value;

            slotLabel += "examine";

            if (selfaware)
                slotLabel += "-selfaware";

            if (!_inventorySystem.TryGetSlotEntity(uid, slotName, out var slotEntity))
                continue;

            if (_entityManager.TryGetComponent<MetaDataComponent>(slotEntity, out var metaData)
                && !HasComp<StripMenuInvisibleComponent>(slotEntity))
            {
                var itemTex = Loc.GetString(slotLabel, ("item", metaData.EntityName), ("ent", uid), ("id", slotEntity.Value.Id), ("size", 14));
                if (showExamine)
                    args.PushMarkup($"[font size=10]{Loc.GetString(slotLabel, ("item", metaData.EntityName), ("ent", uid), ("id", "empty"))}[/font]", priority);
                logLines.Add($"[color=DarkGray][font size=10]{itemTex}[/font][/color]");
                priority--;
            }
        }

        if (priority < 13) // If nothing is worn dont show
        {
            if (showExamine)
                args.PushMarkup($"[font size=10]{cansee}[/font]", 14);
        }
        else
        {
            string canseenothingloc = "examine-can-see-nothing";

            if (selfaware)
                canseenothingloc += "-selfaware";

            var canseenothing = Loc.GetString(canseenothingloc, ("ent", uid));
            logLines.Add($"[color=DarkGray][font size=10]{canseenothing}[/font][/color]");
        }

        FormattedMessage message = new();
        message.PushTag(new MarkupNode("examineborder", null, null)); // border
        message.PushNewline();
        message.AddText($"[color=DarkGray][font size=11]{name}[/font][/color]");
        message.PushNewline();
        AddLine(message);
        foreach (var line in logLines)
        {
            message.AddText(line);
            message.PushNewline();
        }
        AddLine(message);
        message.Pop();
        if (showExamine && _netConfigManager.GetClientCVar(actorComponent.PlayerSession.Channel, GoobCVars.LogInChat))
        {
            _chatManager.ChatMessageToOne(ChatChannel.Emotes, message.ToString(), message.ToMarkup(), EntityUid.Invalid, false, actorComponent.PlayerSession.Channel, recordReplay: false, canCoalesce: false); // Goobstation Edit
        }
    }

    private void HandleExamine(EntityUid uid, MetaDataComponent metaData, ExamineCompletedEvent args)
    {
        if (HasComp<ExaminableCharacterComponent>(args.Examined)
            && !args.IsSecondaryInfo)
            return;

        if (TryComp<ActorComponent>(args.Examiner, out var actorComponent)
            && _netConfigManager.GetClientCVar(actorComponent.PlayerSession.Channel, GoobCVars.DetailedExamine)
            && _netConfigManager.GetClientCVar(actorComponent.PlayerSession.Channel, GoobCVars.LogInChat))
        {
            var logLines = new List<string>();

            FormattedMessage message = new();
            message.PushTag(new MarkupNode("examineborder", null, null)); // border
            message.PushNewline();
            message.Pop();

            if (!args.IsSecondaryInfo)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                var item = Loc.GetString("examine-present-tex", ("name", textInfo.ToTitleCase(metaData.EntityName)), ("id", uid.Id), ("size", 14));
                message.AddText($"[color=DarkGray][font size=11]{item}[/font][/color]");
                message.PushNewline();
            }
            AddLine(message);
            message.AddText($"[font size=10]{args.Message}[/font]");
            message.PushNewline();
            AddLine(message);
            message.Pop();

            _chatManager.ChatMessageToOne(ChatChannel.Emotes, message.ToString(), message.ToMarkup(), EntityUid.Invalid, false, actorComponent.PlayerSession.Channel, recordReplay: false, canCoalesce: false); // Goobstation Edit
        }
    }

    private void AddLine(FormattedMessage message)
    {
        message.PushColor(Color.FromHex("#282D31"));
        message.AddText(Loc.GetString("examine-border-line"));
        message.PushNewline();
        message.Pop();
    }
}
