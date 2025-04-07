// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using System.Collections.Generic;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Pair;

public sealed partial class TestPair
{
    private readonly Dictionary<string, object> _modifiedClientCvars = new();
    private readonly Dictionary<string, object> _modifiedServerCvars = new();

    private void OnServerCvarChanged(CVarChangeInfo args)
    {
        _modifiedServerCvars.TryAdd(args.Name, args.OldValue);
    }

    private void OnClientCvarChanged(CVarChangeInfo args)
    {
        _modifiedClientCvars.TryAdd(args.Name, args.OldValue);
    }

    internal void ClearModifiedCvars()
    {
        _modifiedClientCvars.Clear();
        _modifiedServerCvars.Clear();
    }

    /// <summary>
    /// Reverts any cvars that were modified during a test back to their original values.
    /// </summary>
    public async Task RevertModifiedCvars()
    {
        await Server.WaitPost(() =>
        {
            foreach (var (name, value) in _modifiedServerCvars)
            {
                if (Server.CfgMan.GetCVar(name).Equals(value))
                    continue;
                Server.Log.Info($"Resetting cvar {name} to {value}");
                Server.CfgMan.SetCVar(name, value);
            }

            // I just love order dependent cvars
            if (_modifiedServerCvars.TryGetValue(CCVars.PanicBunkerEnabled.Name, out var panik))
                Server.CfgMan.SetCVar(CCVars.PanicBunkerEnabled.Name, panik);

        });

        await Client.WaitPost(() =>
        {
            foreach (var (name, value) in _modifiedClientCvars)
            {
                if (Client.CfgMan.GetCVar(name).Equals(value))
                    continue;

                var flags = Client.CfgMan.GetCVarFlags(name);
                if (flags.HasFlag(CVar.REPLICATED) && flags.HasFlag(CVar.SERVER))
                    continue;

                Client.Log.Info($"Resetting cvar {name} to {value}");
                Client.CfgMan.SetCVar(name, value);
            }
        });

        ClearModifiedCvars();
    }
}