// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 EmoGarbage404 <retron404@gmail.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Revolutionary.Components; // GoobStation
using Content.Server.Roles;
using Content.Shared.Database;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Revolutionary; // GoobStation
using Content.Shared.Revolutionary.Components;
using Content.Shared.Tag;
using Robust.Shared.Containers;

namespace Content.Server.Mindshield;

/// <summary>
/// System used for checking if the implanted is a Rev or Head Rev.
/// </summary>
public sealed class MindShieldSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedRevolutionarySystem _revolutionarySystem = default!; // Goobstation

    [ValidatePrototypeId<TagPrototype>]
    public const string MindShieldTag = "MindShield";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubdermalImplantComponent, ImplantImplantedEvent>(ImplantCheck);
        SubscribeLocalEvent<MindShieldImplantComponent, EntGotRemovedFromContainerMessage>(OnImplantDraw);
    }

    /// <summary>
    /// Checks if the implant was a mindshield or not
    /// </summary>
    public void ImplantCheck(EntityUid uid, SubdermalImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (!_tag.HasTag(ev.Implant, MindShieldTag) || ev.Implanted == null) // Edited Goobstation
            return;

        EnsureComp<MindShieldComponent>(ev.Implanted.Value);
        MindShieldRemovalCheck(ev.Implanted.Value, ev.Implant);

        // GoobStation
        if (!TryComp<CommandStaffComponent>(ev.Implanted, out var commandComp))
            return;

        commandComp.Enabled = true;
    }

    /// <summary>
    /// Checks if the implanted person was a Rev or Head Rev and remove role or destroy mindshield respectively.
    /// </summary>
    public void MindShieldRemovalCheck(EntityUid implanted, EntityUid implant)
    {
        if (TryComp<HeadRevolutionaryComponent>(implanted, out var headRevComp)) // GoobStation - headRevComp
        {
            _popupSystem.PopupEntity(Loc.GetString("head-rev-break-mindshield"), implanted);
            _revolutionarySystem.ToggleConvertAbility((implanted, headRevComp), false); // GoobStation - turn off headrev ability to convert
            //QueueDel(implant); - Goobstation - Headrevs should remove implant before turning on ability
            return;
        }

        if (_mindSystem.TryGetMind(implanted, out var mindId, out _) &&
            _roleSystem.MindTryRemoveRole<RevolutionaryRoleComponent>(mindId))
        {
            _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(implanted)} was deconverted due to being implanted with a Mindshield.");
        }
        if (HasComp<Goobstation.Shared.Mindcontrol.MindcontrolledComponent>(implanted))   //Goobstation - Mindcontrol Implant
            RemComp<Goobstation.Shared.Mindcontrol.MindcontrolledComponent>(implanted);
    }

    private void OnImplantDraw(Entity<MindShieldImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!_tag.HasTag(ent, MindShieldTag))
            return;

        _popupSystem.PopupEntity(Loc.GetString("mindshield-implant-effect-removed"), args.Container.Owner, args.Container.Owner);

        if (TryComp<HeadRevolutionaryComponent>(args.Container.Owner, out var headRevComp))
            _revolutionarySystem.ToggleConvertAbility((args.Container.Owner, headRevComp), true);

        RemComp<MindShieldComponent>(args.Container.Owner);
    }
}

