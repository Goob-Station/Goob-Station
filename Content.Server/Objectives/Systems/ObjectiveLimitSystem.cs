// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.GameTicking.Rules.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

public sealed class ObjectiveLimitSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObjectiveLimitComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(Entity<ObjectiveLimitComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        if (Prototype(ent)?.ID is not {} proto)
        {
            Log.Error($"ObjectiveLimit used for non-prototyped objective {ent}");
            return;
        }

        var remaining = ent.Comp.Limit;
        // all traitor rules are considered
        // maybe this would interfere with multistation stuff in the future but eh
        foreach (var rule in EntityQuery<TraitorRuleComponent>())
        {
            foreach (var mindId in rule.TraitorMinds)
            {
                if (mindId == args.MindId || !HasObjective(mindId, proto))
                    continue;

                remaining--;

                // limit has been reached, prevent adding the objective
                if (remaining == 0)
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the mind has an objective of a certain prototype.
    /// </summary>
    public bool HasObjective(EntityUid mindId, string proto, MindComponent? mind = null)
    {
        if (!Resolve(mindId, ref mind))
            return false;

        foreach (var objective in mind.Objectives)
        {
            if (Prototype(objective)?.ID == proto)
                return true;
        }

        return false;
    }
}