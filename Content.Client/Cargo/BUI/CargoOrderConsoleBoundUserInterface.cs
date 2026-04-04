// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 windarkata <windarkata@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 R. Neuser <rneuser@iastate.edu>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Marat Gadzhiev <15rinkashikachi15@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 corentt <36075110+corentt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
// SPDX-FileCopyrightText: 2023 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Cargo.UI;
using Content.Shared.Cargo;
using Content.Shared.Cargo.BUI;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Events;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.IdentityManagement;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.Cargo.BUI
{
    public sealed class CargoOrderConsoleBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IPrototypeManager _protoManager = default!; // CorvaxGoob-CargoFeatures
        private readonly SharedCargoSystem _cargoSystem;

        [ViewVariables]
        private CargoConsoleMenu? _menu;

        /// <summary>
        /// This is the separate popup window for individual orders.
        /// </summary>
        [ViewVariables]
        private CargoConsoleOrderMenu? _orderMenu;

        [ViewVariables]
        public string? AccountName { get; private set; }

        [ViewVariables]
        public int BankBalance { get; private set; }

        [ViewVariables]
        public int OrderCapacity { get; private set; }

        [ViewVariables]
        public int OrderCount { get; private set; }

        /// <summary>
        /// Currently selected product
        /// </summary>
        [ViewVariables]
        private CargoProductPrototype? _product;

        public CargoOrderConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            _cargoSystem = EntMan.System<SharedCargoSystem>();
        }

        protected override void Open()
        {
            base.Open();

            var spriteSystem = EntMan.System<SpriteSystem>();

            var dependencies = IoCManager.Instance!;
            _menu = new CargoConsoleMenu(Owner, EntMan, dependencies.Resolve<IPrototypeManager>(), spriteSystem);
            var localPlayer = dependencies.Resolve<IPlayerManager>().LocalEntity;
            var description = new FormattedMessage();

            // CorvaxGoob-CargoFeatures : Алгоритм генерации имени перенесено вниз
            /*string orderRequester;

            if (EntMan.EntityExists(localPlayer))
                orderRequester = Identity.Name(localPlayer.Value, EntMan);
            else
                orderRequester = string.Empty;*/

            _orderMenu = new CargoConsoleOrderMenu();

            _menu.OnClose += Close;

            _menu.OnItemSelected += (args) =>
            {
                if (args.Button.Parent?.Parent is not CargoProductRow row) // Goobstation
                    return;

                _orderMenu.ToggleDepartmentSecureCrate.Pressed = false; // CorvaxGoob-CargoFeatures : дефолт знач при каждом открытии

                description.Clear();
                description.PushColor(Color.White); // Rich text default color is grey
                if (row.MainButton.ToolTip != null)
                    description.AddText(row.MainButton.ToolTip);

                _orderMenu.Description.SetMessage(description);
                _product = row.Product;
                _orderMenu.ProductName.Text = row.ProductName.Text;
                _orderMenu.PointCost.Text = row.PointCost.Text;
                _orderMenu.Amount.Value = 1;

                // CorvaxGoob-CargoFeatures-Start
                if (EntMan.TryGetComponent<CargoOrderConsoleComponent>(Owner, out var orderConsole))
                {
                    _orderMenu.Requester.Editable = orderConsole.EditableRequesterName;

                    if (_protoManager.TryIndex<CargoAccountPrototype>(orderConsole.Account, out var accountPrototype))
                        _orderMenu.DeliveryDestination.PlaceHolder = Loc.GetString(accountPrototype.DepartmentDestinationName ?? "cargo-console-paper-delivery-destination-default");

                    _orderMenu.Requester.Text = localPlayer.HasValue ? _cargoSystem.GenerateRequesterName((Owner, orderConsole), localPlayer.Value) : string.Empty;

                    _orderMenu.ToggleDepartmentSecureCrate.Text = Loc.GetString("cargo-console-secure-order-checkbox", ("cost", orderConsole.SecureOrderCost));

                    if (_product is not null && _protoManager.TryIndex<EntityPrototype>(_product.Product, out var cargoProductEntPrototype))
                        _orderMenu.ToggleDepartmentSecureCrate.Disabled = !_cargoSystem.CanBeSecuredDelivery((Owner, orderConsole), cargoProductEntPrototype);
                }
                // CorvaxGoob-CargoFeatures-End

                _orderMenu.OpenCentered();
            };
            _menu.OnOrderApproved += ApproveOrder;
            _menu.OnOrderCanceled += RemoveOrder;

            _orderMenu.ToggleDepartmentSecureCrate.OnToggled += ToggleDepartmentSecureCrate_OnToggled; // CorvaxGoob-CargoFeatures

            _orderMenu.SubmitButton.OnPressed += (_) =>
            {
                if (AddOrder())
                {
                    _orderMenu.Close();
                }
            };

            _menu.OnAccountAction += (account, amount) =>
            {
                SendMessage(new CargoConsoleWithdrawFundsMessage(account, amount));
            };

            _menu.OnToggleUnboundedLimit += _ =>
            {
                SendMessage(new CargoConsoleToggleLimitMessage());
            };

            _menu.OpenCentered();
        }

        // CorvaxGoob-CargoFeatures-Start
        private void ToggleDepartmentSecureCrate_OnToggled(ButtonToggledEventArgs obj)
        {
            if (_product is null
                || _orderMenu is null
                || !EntMan.TryGetComponent<CargoOrderConsoleComponent>(Owner, out var orderConsole))
                return;

            int cost = obj.Pressed ? _product.Cost + orderConsole.SecureOrderCost : _product.Cost; // Цена либо с защищённым либо нет

            _orderMenu.PointCost.Text = Loc.GetString("cargo-console-menu-points-amount", ("amount", cost));
        }
        // CorvaxGoob-CargoFeatures-End

        private void Populate(List<CargoOrderData> orders)
        {
            if (_menu == null)
                return;

            _menu.PopulateProducts();
            _menu.PopulateCategories();
            _menu.PopulateOrders(orders);
            _menu.PopulateAccountActions();
            _menu.PopulateAccounts(); // CorvaxGoob-CargoFeatures
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not CargoConsoleInterfaceState cState || !EntMan.TryGetComponent<CargoOrderConsoleComponent>(Owner, out var orderConsole))
                return;
            var station = EntMan.GetEntity(cState.Station);

            OrderCapacity = cState.Capacity;
            OrderCount = cState.Count;
            BankBalance = _cargoSystem.GetBalanceFromAccount(station, orderConsole.Account);

            AccountName = cState.Name;

            if (_menu == null)
                return;

            _menu.ProductCatalogue = cState.Products;

            _menu?.UpdateStation(station);
            Populate(cState.Orders);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            _menu?.Dispose();
            _orderMenu?.Dispose();
        }

        private bool AddOrder()
        {
            // CorvaxGoob-CargoFeatures
            if (!EntMan.TryGetComponent<CargoOrderConsoleComponent>(Owner, out var orderConsole))
                return false;

            var orderAmt = _orderMenu?.Amount.Value ?? 0;
            if (orderAmt < 1 || orderAmt > OrderCapacity)
            {
                return false;
            }

            SendMessage(new CargoConsoleAddOrderMessage(
                orderConsole.EditableRequesterName ? _orderMenu?.Requester.Text : null, // CorvaxGoob-CargoFeatures
                _orderMenu?.DeliveryDestination.Text == "" ? _orderMenu?.DeliveryDestination.PlaceHolder : _orderMenu?.DeliveryDestination.Text, // CorvaxGoob-CargoFeatures
                _orderMenu?.Note.Text == "" ? null : _orderMenu?.Note.Text, // CorvaxGoob-CargoFeatures
                _product?.ID ?? "",
                orderAmt,
                _orderMenu?.ToggleDepartmentSecureCrate.Pressed ?? false)); // CorvaxGoob-CargoFeatures

            return true;
        }

        private void RemoveOrder(ButtonEventArgs args)
        {
            if (args.Button.Parent?.Parent?.Parent is not CargoOrderRow row || row.Order == null) // Goobstation
                return;

            SendMessage(new CargoConsoleRemoveOrderMessage(row.Order.OrderId));
        }

        private void ApproveOrder(ButtonEventArgs args)
        {
            if (args.Button.Parent?.Parent?.Parent is not CargoOrderRow row || row.Order == null) // Goobstation
                return;

            if (OrderCount >= OrderCapacity)
                return;

            SendMessage(new CargoConsoleApproveOrderMessage(row.Order.OrderId));
        }
    }
}
