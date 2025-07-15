using Content.Goobstation.Shared.GameTicking;

namespace Content.Goobstation.Server.SlaughterDemon.Items;

/// <summary>
/// This handles attaching the wizard to the slaughter demon's objective
/// </summary>
public sealed class VialSummonSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<VialSummonComponent, AddGameRuleItemEvent>(OnAddGameRule);
    }

    private void OnAddGameRule(Entity<VialSummonComponent> ent, ref AddGameRuleItemEvent args)
    {
        ent.Comp.Summoner = args.Initiator;
    }
}
