// SPDX-FileCopyrightText: 2019 Remie Richards <remierichards@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2019 ZelteHonor <gabrieldionbouchard@gmail.com>
// SPDX-FileCopyrightText: 2019 moneyl <8206401+Moneyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Popups;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Verbs;

namespace Content.Server.Verbs
{
    public sealed class VerbSystem : SharedVerbSystem
    {
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly IAdminManager _adminMgr = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<RequestServerVerbsEvent>(HandleVerbRequest);
        }

        private void HandleVerbRequest(RequestServerVerbsEvent args, EntitySessionEventArgs eventArgs)
        {
            var player = eventArgs.SenderSession;

            if (!EntityManager.EntityExists(GetEntity(args.EntityUid)))
            {
                Log.Warning($"{nameof(HandleVerbRequest)} called on a non-existent entity with id {args.EntityUid} by player {player}.");
                return;
            }

            if (player.AttachedEntity is not {} attached)
            {
                Log.Warning($"{nameof(HandleVerbRequest)} called by player {player} with no attached entity.");
                return;
            }

            // We do not verify that the user has access to the requested entity. The individual verbs should check
            // this, and some verbs (e.g. view variables) won't even care about whether an entity is accessible through
            // the entity menu or not.

            var force = args.AdminRequest && eventArgs.SenderSession is { } playerSession &&
                        _adminMgr.HasAdminFlag(playerSession, AdminFlags.Admin);

            List<Type> verbTypes = new();
            foreach (var key in args.VerbTypes)
            {
                var type = Verb.VerbTypes.FirstOrDefault(x => x.Name == key);

                if (type != null)
                    verbTypes.Add(type);
                else
                    Log.Error($"Unknown verb type received: {key}");
            }

            var response =
                new VerbsResponseEvent(args.EntityUid, GetLocalVerbs(GetEntity(args.EntityUid), attached, verbTypes, force));
            RaiseNetworkEvent(response, player.Channel);
        }

        /// <summary>
        ///     Execute the provided verb.
        /// </summary>
        /// <remarks>
        ///     This will try to call the action delegates and raise the local events for the given verb.
        /// </remarks>
        public override void ExecuteVerb(Verb verb, EntityUid user, EntityUid target, bool forced = false)
        {
            // is this verb actually valid?
            if (verb.Disabled)
            {
                // Send an informative pop-up message
                if (!string.IsNullOrWhiteSpace(verb.Message))
                    _popupSystem.PopupEntity(verb.Message, user, user);

                return;
            }

            // first, lets log the verb. Just in case it ends up crashing the server or something.
            LogVerb(verb, user, target, forced);

            base.ExecuteVerb(verb, user, target, forced);
        }

        public void LogVerb(Verb verb, EntityUid user, EntityUid target, bool forced)
        {
            // first get the held item. again.
            EntityUid? holding = null;
            if (TryComp(user, out HandsComponent? hands) &&
                hands.ActiveHandEntity is EntityUid heldEntity)
            {
                holding = heldEntity;
            }

            // if this is a virtual pull, get the held entity
            if (holding != null && TryComp(holding, out VirtualItemComponent? pull))
                holding = pull.BlockingEntity;

            var verbText = $"{verb.Category?.Text} {verb.Text}".Trim();

            // lets not frame people, eh?
            var executionText = forced ? "was forced to execute" : "executed";

            if (holding == null)
            {
                _adminLogger.Add(LogType.Verb, verb.Impact,
                        $"{ToPrettyString(user):user} {executionText} the [{verbText:verb}] verb targeting {ToPrettyString(target):target}");
            }
            else
            {
                _adminLogger.Add(LogType.Verb, verb.Impact,
                       $"{ToPrettyString(user):user} {executionText} the [{verbText:verb}] verb targeting {ToPrettyString(target):target} while holding {ToPrettyString(holding.Value):held}");
            }
        }
    }
}