// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vyacheslav Titov <rincew1nd@ya.ru>
// SPDX-FileCopyrightText: 2023 Титов Вячеслав Витальевич <rincew1nd@yandex.ru>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Store;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Allows a store entry to be filtered out based on the user's antag role.
/// Supports both blacklists and whitelists. This is copypaste because roles
/// are absolute shitcode. Refactor this later. -emo
/// </summary>
public sealed partial class BuyerAntagCondition : ListingCondition
{
    /// <summary>
    /// A whitelist of antag roles that can purchase this listing. Only one needs to be found.
    /// </summary>
    [DataField("whitelist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<AntagPrototype>))]
    public HashSet<string>? Whitelist;

    /// <summary>
    /// A blacklist of antag roles that cannot purchase this listing. Only one needs to be found.
    /// </summary>
    [DataField("blacklist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<AntagPrototype>))]
    public HashSet<string>? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;

        if (!ent.HasComponent<MindComponent>(args.Buyer))
            return true; // inanimate objects don't have minds

        var roleSystem = ent.System<SharedRoleSystem>();
        var roles = roleSystem.MindGetAllRoleInfo(args.Buyer);

        if (Blacklist != null)
        {
            foreach (var role in roles)
            {
                if (!role.Antagonist || string.IsNullOrEmpty(role.Prototype))
                    continue;

                if (Blacklist.Contains(role.Prototype))
                    return false;
            }
        }

        if (Whitelist != null)
        {
            var found = false;
            foreach (var role in roles)
            {

                if (!role.Antagonist || string.IsNullOrEmpty(role.Prototype))
                    continue;

                if (Whitelist.Contains(role.Prototype))
                    found = true;
            }
            if (!found)
                return false;
        }

        return true;
    }
}