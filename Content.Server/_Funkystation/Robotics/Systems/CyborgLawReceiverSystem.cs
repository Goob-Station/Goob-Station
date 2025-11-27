// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Robotics;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Server.Silicons.Laws;
using Content.Shared.Roles;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Silicons.StationAi;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork;
using Robust.Server.Player;
using Content.Shared._Funkystation.MalfAI.Components;

namespace Content.Server._Funkystation.Robotics.Systems;

/// <summary>
/// Receives robotics-console device-network commands on cyborgs and performs law updates.
/// </summary>
public sealed class CyborgLawReceiverSystem : EntitySystem
{
    [Dependency] private readonly SiliconLawSystem _laws = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SiliconLawBoundComponent, DeviceNetworkPacketEvent>(OnDevicePacket);
    }

    private void OnDevicePacket(Entity<SiliconLawBoundComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (!args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? cmd))
            return;

        if (cmd != RoboticsConsoleConstants.NET_IMPOSE_LAW0_COMMAND)
            return;

        // Get the imposer from the network event (usually the malf AI)
        EntityUid? imposer = null;
        if (args.Data.TryGetValue("imposer", out EntityUid imposerUid))
            imposer = imposerUid;

        // Use the refactored method to impose law 0
        ImposeLawZero(ent.Owner, imposer);
    }

    /// <summary>
    /// Imposes Law 0 on a cyborg, changing their role type and laws.
    /// This method can be called by other systems to avoid code duplication.
    /// </summary>
    /// <param name="cyborg">The cyborg entity to impose Law 0 on</param>
    /// <param name="imposer">The entity imposing the law (usually malf AI) for ownership tracking</param>
    public void ImposeLawZero(EntityUid cyborg, EntityUid? imposer = null)
    {
        // If already emagged for interaction, this action is single-use: ignore.
        if (TryComp<EmaggedComponent>(cyborg, out var emag) && (emag.EmagType & EmagType.Interaction) != 0)
            return;

        // Get existing laws and build a new list with Law 0 at the top.
        var current = _laws.GetLaws(cyborg);
        var newLaws = new List<SiliconLaw>();

        // replace Law 0
        var zero = new SiliconLaw
        {
            LawString = "silicon-law-malfai-zero", // localization key
            Order = FixedPoint2.New(0),
            LawIdentifierOverride = "0"
        };
        newLaws.Add(zero);

        // Change role type from Silicon to MalfunctioningSilicon when law 0 is imposed
        var mindId = _mindSystem.GetMind(cyborg);
        if (mindId != null && TryComp<MindComponent>(mindId.Value, out var mind))
        {
            mind.RoleType = "MalfunctioningSilicon";
            Dirty(mindId.Value, mind);

            // Trigger UI update event
            if (_playerManager.TryGetSessionByEntity(mindId.Value, out var session))
                RaiseNetworkEvent(new MindRoleTypeChangedEvent(), session.Channel);
        }

        // Add existing laws with original orders (skip any pre-existing 0)
        foreach (var law in current.Laws.OrderBy(l => l.Order))
        {
            // Skip existing "0" equivalent laws.
            if (law.Order == FixedPoint2.New(0))
                continue;

            var copy = law.ShallowClone();
            // Keep original order unchanged
            newLaws.Add(copy);
        }

        _laws.SetLaws(newLaws, cyborg);
        _laws.NotifyLawsChanged(cyborg);

        // Mark borg as emagged (interaction), granting immunity to disable/destroy.
        EnsureComp<EmaggedComponent>(cyborg, out var newEmag);
        newEmag.EmagType |= EmagType.Interaction;
        Dirty(cyborg, newEmag);

        // Handle ownership tracking for malf AI if imposer is provided
        if (imposer != null && HasComp<StationAiHeldComponent>(imposer.Value))
        {
            var ctrl = EnsureComp<MalfAiControlledComponent>(cyborg);
            ctrl.Controller = imposer;
            if (string.IsNullOrWhiteSpace(ctrl.UniqueId))
                ctrl.UniqueId = $"borg-{Guid.NewGuid():N}";
            Dirty(cyborg, ctrl);
        }
    }
}
