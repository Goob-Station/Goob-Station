using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Server.GameTicking;
using Content.Server.Revolutionary.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Clothing.Systems;

/// <summary>
/// This handles clothign that grant server currency for items if worn on end of round
/// </summary>
public sealed class ClothingGrantServerCurrencySystem : EntitySystem
{

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ICommonCurrencyManager _currencyMan = default!;


    private int _goobcoinsMinPlayers;

    public override void Initialize()
    {

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);

        Subs.CVar(_cfg, GoobCVars.GoobcoinMinPlayers, value => _goobcoinsMinPlayers = value, true);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (_players.PlayerCount < _goobcoinsMinPlayers)
            return;

        var query = EntityQueryEnumerator<ClothingGrantServerCurrencyComponent, ClothingComponent>();

        while (query.MoveNext(out var uid, out var comp,out var clothing))
        {
            if (!_inventory.InSlotWithFlags((uid ,Transform(uid),MetaData(uid)), comp.Slot))
                continue;
            if (!_inventory.TryGetContainingEntity(uid, out var wearer))
                continue;

            if(HasComp<CommandStaffComponent>(wearer.Value)) //no medals for command
                continue;

            if(!TryComp<MindContainerComponent>(wearer.Value, out var mindContainer))
                continue;
            if(!mindContainer.Mind.HasValue)
                continue;
            var mind = Comp<MindComponent>(mindContainer.Mind.Value);
            if(!mind.OriginalOwnerUserId.HasValue)
                continue;

            _currencyMan.AddCurrency(mind.OriginalOwnerUserId.Value, comp.Amount);
        }
    }
}
