// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Shared.Store.Components;
using System.Linq;
using Content.Server.Administration;

// i made this command cuz i was tired of waiting, so its for debug purposes
namespace Content.Goobstation.Server.Administration
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class AddStoreTimeCommand : IConsoleCommand
    {
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "addstoretime";
        public string Description => "Reduces store listing restock time by specified seconds, DEBUG";
        public string Help => "Usage: addstoretime <seconds> <storeUid> <listingId>, for example NTRExecutiveEdagger, NTRExecutiveBSDCall, etc";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not { } player)
            {
                shell.WriteLine("This command cannot be run from server console.");
                return;
            }

            if (!_adminManager.HasAdminFlag(player, AdminFlags.Admin))
            {
                shell.WriteLine("You don't have permission to run this command!");
                return; // who do you think i am to let players abuse this command, john station mr. 65%?
            }

            if (args.Length != 3)
            {
                shell.WriteLine("Invalid number of arguments!");
                shell.WriteLine(Help);
                return;
            }

            if (!int.TryParse(args[0], out var seconds) || seconds <= 0)
            {
                shell.WriteLine("Invalid time value! Must be positive integer seconds.");
                return;
            }

            if (!EntityUid.TryParse(args[1], out var storeUid))
            {
                shell.WriteLine("Invalid store entity UID!");
                return;
            }

            var listingId = args[2];

            if (!_entities.TryGetComponent<StoreComponent>(storeUid, out var store))
            {
                shell.WriteLine("Target entity is not a store!");
                return;
            }

            var listing = store.Listings.FirstOrDefault(l => l.ID == listingId);

            if (listing == null)
            {
                shell.WriteLine($"Listing with ID '{listingId}' not found in store!");
                return;
            }

            if (listing.RestockTime > TimeSpan.Zero)
            {
                var newRestockTime = listing.RestockTime - TimeSpan.FromSeconds(seconds);
                listing.RestockTime = newRestockTime > TimeSpan.Zero ? newRestockTime : TimeSpan.Zero;
                shell.WriteLine($"Restock time reduced by {seconds} seconds for listing '{listingId}'. " +
                               $"New restock time: {listing.RestockTime}");
            }
            else
            {
                shell.WriteLine("Listing is not awaiting restock currently.");
            }
            // todo: UI hot reload
        }
    }
}
