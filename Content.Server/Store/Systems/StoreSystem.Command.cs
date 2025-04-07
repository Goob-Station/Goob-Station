// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.FixedPoint;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Store.Components;
using Robust.Shared.Console;

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;

    public void InitializeCommand()
    {
        _consoleHost.RegisterCommand("addcurrency", "Adds currency to the specified store", "addcurrency <uid> <currency prototype> <amount>",
            AddCurrencyCommand,
            AddCurrencyCommandCompletions);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void AddCurrencyCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError("Argument length must be 3");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var uidNet) || !TryGetEntity(uidNet, out var uid) || !float.TryParse(args[2], out var id))
        {
            return;
        }

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        var currency = new Dictionary<string, FixedPoint2>
        {
            { args[1], id }
        };

        TryAddCurrency(currency, uid.Value, store);
    }

    private CompletionResult AddCurrencyCommandCompletions(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var query = EntityQueryEnumerator<StoreComponent>();
            var allStores = new List<string>();
            while (query.MoveNext(out var storeuid, out _))
            {
                allStores.Add(storeuid.ToString());
            }
            return CompletionResult.FromHintOptions(allStores, "<uid>");
        }

        if (args.Length == 2 && NetEntity.TryParse(args[0], out var uidNet) && TryGetEntity(uidNet, out var uid))
        {
            if (TryComp<StoreComponent>(uid, out var store))
                return CompletionResult.FromHintOptions(store.CurrencyWhitelist.Select(p => p.ToString()), "<currency prototype>");
        }

        return CompletionResult.Empty;
    }
}