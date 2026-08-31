// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Animations;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.SupermatterHalberd;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ActionTargetMarkSystem _mark = default!;
    [Dependency] private readonly RaysSystem _rays = default!;

    public event Action? StopTargeting;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StopTargetingEvent>(OnStopTargeting);
    }

    public void SetSwapSecondaryTarget(EntityUid user, EntityUid? target, EntityUid action)
    {
        if (target == null || user == target)
        {
            _mark.SetMark(null);
            RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), null));
            return;
        }

        _mark.SetMark(target);
        RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), GetNetEntity(target.Value)));
    }

    private void OnStopTargeting(StopTargetingEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession != _player.LocalSession)
            return;

        StopTargeting?.Invoke();
    }
}