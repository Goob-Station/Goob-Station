// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Shitmed.GameTicking.Rules.Components;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Robust.Shared.Player;
namespace Content.Server._Shitmed.Antags.Abductor;

public sealed partial class AbductorSystem : SharedAbductorSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    private static readonly string DefaultAbductorVictimRule = "AbductorVictim";
    public void InitializeVictim()
    {
        SubscribeLocalEvent<AbductorComponent, SurgeryStepEvent>(OnSurgeryStepComplete);
    }
    private void OnSurgeryStepComplete(EntityUid uid, AbductorComponent comp, ref SurgeryStepEvent args)
    {
        if (!HasComp<SurgeryAddOrganStepComponent>(args.Step)
            || !args.Complete
            || HasComp<AbductorComponent>(args.Body)
            || !TryComp<AbductorVictimComponent>(args.Body, out var victimComp)
            || victimComp.Implanted
            || !HasComp<HumanoidAppearanceComponent>(args.Body)
            || !_mind.TryGetMind(args.Body, out var mindId, out var mind)
            || !TryComp<ActorComponent>(args.Body, out var actor))
            return;

        if (mindId == default
            || !_role.MindHasRole<AbductorVictimRoleComponent>(mindId, out _))
        {
            _role.MindAddRole(mindId, "MindRoleAbductorVictim");
            victimComp.Implanted = true;
            _antag.ForceMakeAntag<AbductorVictimRuleComponent>(actor.PlayerSession, DefaultAbductorVictimRule);

            _adminLogManager.Add(LogType.Mind,
                LogImpact.Medium,
                $"{ToPrettyString(args.User)} has given {ToPrettyString(args.Body)} an abductee objective.");

        }

    }
}