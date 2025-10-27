// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Database;
using Content.Shared.CCVar;
using Robust.Server.Upload;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Upload;

namespace Content.Server.Administration;

public sealed class ContentNetworkResourceManager
{
    [Dependency] private readonly IServerDbManager _serverDb = default!;
    [Dependency] private readonly NetworkResourceManager _netRes = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;

    [ViewVariables] public bool StoreUploaded { get; set; } = true;

    public void Initialize()
    {
        _cfgManager.OnValueChanged(CCVars.ResourceUploadingStoreEnabled, value => StoreUploaded = value, true);
        AutoDelete(_cfgManager.GetCVar(CCVars.ResourceUploadingStoreDeletionDays));
        _netRes.OnResourceUploaded += OnUploadResource;
    }

    private async void OnUploadResource(ICommonSession session, NetworkResourceUploadMessage msg)
    {
        if (StoreUploaded)
            await _serverDb.AddUploadedResourceLogAsync(session.UserId, DateTime.Now, msg.RelativePath.ToString(), msg.Data);
    }

    private async void AutoDelete(int days)
    {
        if (days > 0)
            await _serverDb.PurgeUploadedResourceLogAsync(days);
    }
}