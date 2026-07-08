// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.GameTicking;
using Content.Goobstation.Shared.SlaughterDemon.Items;

namespace Content.Goobstation.Server.SlaughterDemon.Items;

/// <summary>
/// This handles attaching the wizard to the slaughter demon's objective
/// </summary>
public sealed class VialSummonSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VialSummonComponent, AddGameRuleItemEvent>(OnAddGameRule);
    }

    private void OnAddGameRule(Entity<VialSummonComponent> ent, ref AddGameRuleItemEvent args) =>
        ent.Comp.Summoner = args.Initiator;
}
