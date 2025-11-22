using Robust.Shared.Player;

namespace Content.Server._CorvaxGoob.Players.GhostTimeTracking;
public sealed class GhostTimeTrackingManager
{
    private readonly Dictionary<ICommonSession, TimeSpan> _ghostTimeData = new();

    /// <summary>
    /// Returns player's played time as ghost.
    /// </summary>
    public TimeSpan GetPlayerGhostTime(ICommonSession session)
    {
        return _ghostTimeData.GetValueOrDefault(session);
    }

    /// <summary>
    /// Clears all players played time data as ghost.
    /// </summary>
    public void ClearAllGhostTimeData()
    {
        _ghostTimeData.Clear();
    }

    /// <summary>
    /// Adds player's played time as ghost.
    /// </summary>
    public void UpdatePlayerGhostTime(ICommonSession session, TimeSpan ghostTime)
    {
        if (!_ghostTimeData.TryGetValue(session, out var ghostTimeData)) // if there's no key, create new onew with 1 second in data
        {
            _ghostTimeData.Add(session, ghostTime);
            return;
        }

        _ghostTimeData[session] = ghostTimeData + ghostTime; // in other way just add time into existed keyGetPlayerGhostTime
    }
}
