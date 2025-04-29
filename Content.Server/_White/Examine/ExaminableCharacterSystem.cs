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
using System.Globalization;

namespace Content.Server._White.Examine
{
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
            logLines.Add($"[color=DarkGray][font size=10]{name}[/font][/color]");

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
                    var item = Loc.GetString(slotLabel, ("item", metaData.EntityName), ("ent", uid));
                    if (showExamine)
                        args.PushMarkup($"[font size=10]{item}[/font]", priority);
                    logLines.Add($"[color=DarkGray][font size=10]{item}[/font][/color]");
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

            var combinedLog = string.Join("\n", logLines);

            if (showExamine && _netConfigManager.GetClientCVar(actorComponent.PlayerSession.Channel, GoobCVars.LogInChat))
                _chatManager.ChatMessageToOne(ChatChannel.Emotes, combinedLog, combinedLog, EntityUid.Invalid, false, actorComponent.PlayerSession.Channel, recordReplay: false);
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

                if (!args.IsSecondaryInfo)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    logLines.Add($"[color=DarkGray][font size=11]{textInfo.ToTitleCase(metaData.EntityName)}[/font][/color]");
                }

                logLines.Add($"[color=DarkGray][font size=10]{args.Message}[/font][/color]");
                var combinedLog = string.Join("\n", logLines);
                _chatManager.ChatMessageToOne(ChatChannel.Emotes, combinedLog, combinedLog, EntityUid.Invalid, false, actorComponent.PlayerSession.Channel, recordReplay: false);
            }
        }
    }
}
