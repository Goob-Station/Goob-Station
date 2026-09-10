using Content.Shared._DV.Salvage.Components;
using Content.Shared._DV.Salvage.Systems;
using Content.Shared.Materials.OreSilo;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Lathe.UI;

public sealed partial class LatheMenu
{
    [Dependency] private readonly IPlayerManager _player = default!;
    private readonly MiningPointsSystem _miningPoints;

    private uint? _lastMiningPoints; // DeltaV: used to avoid Loc.GetString every frame

    public event Action<BaseButton.ButtonEventArgs>? OnResetQueueListButtonPressed;
    public event Action? OnClaimMiningPoints;

    private void UpdateMiningPoints(uint points)
    {
        MiningPointsClaimButton.Disabled = points == 0 ||
            _player.LocalSession?.AttachedEntity is not { } player ||
            !_miningPoints.CanClaimPoints(player);
        if (points == _lastMiningPoints)
            return;

        _lastMiningPoints = points;
        MiningPointsLabel.Text = Loc.GetString("lathe-menu-mining-points", ("points", points));
    }

    /// <summary>
    /// Goobstation: Check if the lathe is connected to a silo.
    /// TODO: we have to account for silo range, here and in OnClaimMiningPoints
    /// </summary>
    private string? GetMiningPointsWarning(EntityUid uid, bool checkGrid = false)
    {
        if (!_entityManager.TryGetComponent<OreSiloClientComponent>(uid, out var siloComp))
            return Loc.GetString("lathe-menu-mining-points-no-connection-warning");
        else if (siloComp.Silo is null)
            return Loc.GetString("lathe-menu-mining-points-no-connection-warning");

        if (siloComp.Silo is { Valid: false })
            return Loc.GetString("lathe-menu-mining-points-silo-invalid");

        // TODO: lathe-menu-mining-points-silo-out-of-range
        // needs code in redeeming points too though, so no warning being shown is correct since it still works
        // when out of silo range

        if (checkGrid)
        {
            if (_entityManager.TryGetComponent<TransformComponent>(uid, out var uidTransform)
                && _entityManager.TryGetComponent<TransformComponent>(siloComp!.Silo, out var siloTransform))
            {
                if (uidTransform.MapID != siloTransform.MapID)
                    return Loc.GetString("lathe-menu-mining-points-silo-not-on-same-map");

                if (uidTransform.GridUid != siloTransform.GridUid && uidTransform.GridUid != null)
                    return Loc.GetString("lathe-menu-mining-points-silo-not-on-same-grid");
            }
        }

        return null;
    }

    /// <summary>
    /// DeltaV: Update mining points UI whenever it changes.
    /// </summary>
    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_entityManager.TryGetComponent<MiningPointsComponent>(Entity, out var points))
            UpdateMiningPoints(points.Points);
    }

    // DeltaV Mining points UI
    private void SetEntityGoob(EntityUid uid)
    {
        _entityManager.TryGetComponent<MiningPointsComponent>(Entity, out var points);

        if (points != null)
        {
            MiningPointsContainer.Visible = true;
            MiningPointsClaimButton.OnPressed += _ => OnClaimMiningPoints?.Invoke();

            UpdateMiningPoints(points.Points);

            if (GetMiningPointsWarning(Entity, true) is { } warning)
            {
                MiningPointsNoConnectionWarning.Visible = true;
                MiningPointsNoConnectionWarning.SetMessage(FormattedMessage.FromMarkupOrThrow(warning));
            }
            else
            {
                MiningPointsNoConnectionWarning.Visible = false;
            }
        }
        else
        {
            MiningPointsContainer.Visible = false;
        }
    }
}