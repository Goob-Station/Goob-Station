using Content.Shared.Store;
using JetBrains.Annotations;
using System.Linq;
using Content.Shared.Store.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.Store.Ui;

[UsedImplicitly]
public sealed class StoreBoundUserInterface : BoundUserInterface
{
    private IPrototypeManager _prototypeManager = default!;

    [ViewVariables]
    private StoreMenu? _menu;

    [ViewVariables]
    private string _search = string.Empty;

    [ViewVariables]
    private HashSet<ListingDataWithCostModifiers> _listings = new();

    public StoreBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    private bool IsMalfAiStore(StoreUpdateState? msg = null)
    {
        if (EntMan.HasComponent<Content.Shared.MalfAI.MalfAiMarkerComponent>(Owner))
            return true;

        if (EntMan.TryGetComponent<StoreComponent>(Owner, out var store) && store.Name == "store-preset-name-malfai")
            return true;

        if (msg != null)
        {
            if (msg.Balance.ContainsKey("CPU"))
                return true;

            if (msg.Listings.Any(l => l.Categories.Contains("Deception") ||
                                      l.Categories.Contains("Factory") ||
                                      l.Categories.Contains("Disruption")))
                return true;
        }

        return false;
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<StoreMenu>();
        if (IsMalfAiStore())
        {
            _menu.Title = Loc.GetString("store-preset-name-malfai");
            _menu.ApplyMalfTheme();
        }
        else if (EntMan.TryGetComponent<StoreComponent>(Owner, out var store))
        {
            _menu.Title = Loc.GetString(store.Name);
            if (store.Name == "store-preset-name-malfai")
                _menu.ApplyMalfTheme();
        }

        _menu.OnListingButtonPressed += (_, listing) =>
        {
            SendMessage(new StoreBuyListingMessage(listing.ID));
        };

        _menu.OnCategoryButtonPressed += (_, category) =>
        {
            _menu.CurrentCategory = category;
            UpdateListingsWithSearchFilter();
        };

        _menu.OnWithdrawAttempt += (_, type, amount) =>
        {
            SendMessage(new StoreRequestWithdrawMessage(type, amount));
        };

        _menu.SearchTextUpdated += (_, search) =>
        {
            _search = search.Trim().ToLowerInvariant();
            UpdateListingsWithSearchFilter();
        };

        _menu.OnRefundAttempt += (_) =>
        {
            SendMessage(new StoreRequestRefundMessage());
        };
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case StoreUpdateState msg:
                if (IsMalfAiStore(msg))
                {
                    _menu?.ApplyMalfTheme();
                    if (_menu != null)
                        _menu.Title = Loc.GetString("store-preset-name-malfai");
                }

                _listings = msg.Listings;

                _menu?.UpdateBalance(msg.Balance);

                UpdateListingsWithSearchFilter();
                _menu?.SetFooterVisibility(msg.ShowFooter);
                _menu?.UpdateRefund(msg.AllowRefund);
                break;
        }
    }

    private void UpdateListingsWithSearchFilter()
    {
        if (_menu == null)
            return;

        var filteredListings = new HashSet<ListingDataWithCostModifiers>(_listings);
        if (!string.IsNullOrEmpty(_search))
        {
            filteredListings.RemoveWhere(listingData => !ListingLocalisationHelpers.GetLocalisedNameOrEntityName(listingData, _prototypeManager).Trim().ToLowerInvariant().Contains(_search) &&
                                                        !ListingLocalisationHelpers.GetLocalisedDescriptionOrEntityDescription(listingData, _prototypeManager).Trim().ToLowerInvariant().Contains(_search));
        }
        _menu.PopulateStoreCategoryButtons(filteredListings);
        _menu.UpdateListing(filteredListings.ToList());
    }
}
