// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Guardian;

public sealed partial class GuardianSystem
{
    // Flag to prevent recursive position updates
    private readonly HashSet<EntityUid> _updatingGuardians = new();

    /// <summary>
    /// Retract the guardian if either the host or the guardian move away from each other.
    /// </summary>
    private void CheckGuardianMove(
        EntityUid hostUid,
        EntityUid guardianUid,
        GuardianHostComponent? hostComponent = null,
        GuardianComponent? guardianComponent = null,
        TransformComponent? hostXform = null,
        TransformComponent? guardianXform = null)
    {
        if (TerminatingOrDeleted(guardianUid) || TerminatingOrDeleted(hostUid))
            return;

        if (!Resolve(hostUid, ref hostComponent, ref hostXform)
            || !Resolve(guardianUid, ref guardianComponent, ref guardianXform))
            return;

        if (!guardianComponent.GuardianLoose || _updatingGuardians.Contains(guardianUid))
            return;

        // Now moves you closer instead of retracting
        if (!_transform.InRange(guardianXform.Coordinates, hostXform.Coordinates, guardianComponent.DistanceAllowed))
        {
            _updatingGuardians.Add(guardianUid);
            try
            {
                if (hostXform.MapID != guardianXform.MapID)
                {
                    _transform.SetCoordinates(guardianUid, guardianXform, hostXform.Coordinates);
                }
                else
                {
                    // host's position in our parent's coordinates
                    var hostPos = _transform.WithEntityId(hostXform.Coordinates, guardianXform.ParentUid).Position;
                    var diff = guardianXform.LocalPosition - hostPos;
                    var newDiff = diff.Normalized() * guardianComponent.DistanceAllowed * 0.9f; // 90% of max distance to prevent jitter
                    _transform.SetLocalPosition(guardianUid, hostPos + newDiff, guardianXform);
                }
            }
            finally
            {
                _updatingGuardians.Remove(guardianUid);
            }
        }
    }
}
