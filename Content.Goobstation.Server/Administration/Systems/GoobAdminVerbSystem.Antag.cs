// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Blob;
using Content.Goobstation.Server.Changeling.GameTicking.Rules;
using Content.Goobstation.Server.Devil.GameTicking.Rules;
using Content.Goobstation.Server.Silicon.MalfAI.Rules;
using Content.Server.Administration.Managers;
using Content.Server.Antag;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed partial class GoobAdminVerbSystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IAdminManager _admin = default!;

    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!AntagVerbAllowed(args, out var targetPlayer))
            return;

        // Malf AI
        if (HasComp<StationAiHolderComponent>(args.Target))
        {
            Verb verb = new()
            {
                Text = Loc.GetString("admin-verb-text-make-malf"),
                Category = VerbCategory.Antag,
                Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Interface/Actions/actions_malfai.rsi"), "modules_menu"),
                Act = () =>
                {
                    _antag.ForceMakeAntag<MalfAIRuleComponent>(targetPlayer, "MalfAIMidround");
                },
                Impact = LogImpact.High,
                Message = Loc.GetString("admin-verb-make-malf"),
            };

            args.Verbs.Add(verb);
            return;
        }

        // Changelings
        Verb ling = new()
        {
            Text = Loc.GetString("admin-verb-text-make-changeling"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Changeling/changeling_abilities.rsi"), "transform"),
            Act = () =>
            {
                if (!HasComp<SiliconComponent>(args.Target))
                    _antag.ForceMakeAntag<ChangelingRuleComponent>(targetPlayer, "Changeling");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-changeling"),
        };
        if (!HasComp<SiliconComponent>(args.Target))
            args.Verbs.Add(ling);

        // Blob
        Verb blobAntag = new()
        {
            Text = Loc.GetString("admin-verb-text-make-blob"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Blob/Actions/blob.rsi"), "blobFactory"),
            Act = () =>
            {
                EnsureComp<BlobCarrierComponent>(args.Target).HasMind = HasComp<ActorComponent>(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-text-make-blob"),
        };
        if (!HasComp<SiliconComponent>(args.Target))
            args.Verbs.Add(blobAntag);

        // Devil
        Verb devilAntag = new()
        {
            Text = Loc.GetString("admin-verb-text-make-devil"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "summon-contract"),
            Act = () =>
            {
                _antag.ForceMakeAntag<DevilRuleComponent>(targetPlayer, "Devil");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-devil"),
        };
        args.Verbs.Add(devilAntag);
    }

    public bool AntagVerbAllowed(GetVerbsEvent<Verb> args, [NotNullWhen(true)] out ICommonSession? target)
    {
        target = null;

        if (!TryComp<ActorComponent>(args.User, out var actor))
            return false;

        var player = actor.PlayerSession;

        if (!_admin.HasAdminFlag(player, AdminFlags.Fun))
            return false;

        if
        (
            TryComp<StationAiHolderComponent>(args.Target, out var aiHolder)
            && aiHolder.Slot.ContainerSlot != null
            && aiHolder.Slot.ContainerSlot.ContainedEntities.Count != 0
            && TryComp<ActorComponent>(aiHolder.Slot.ContainerSlot.ContainedEntities[0], out var aiActor)
        )
        {
            target = aiActor.PlayerSession;
            return true;
        }

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return false;

        target = targetActor.PlayerSession;
        return true;
    }
}
