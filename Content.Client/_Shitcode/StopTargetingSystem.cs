// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Wizard.Events;
using Robust.Client.Player;

namespace Content.Client._Shitcode;

public sealed partial class StopTargetingSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StopTargetingEvent>(OnStopTargeting);
    }

    public event Action? StopTargeting;

    private void OnStopTargeting(StopTargetingEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession != _playerManager.LocalSession)
            return;

        StopTargeting?.Invoke();
    }
}