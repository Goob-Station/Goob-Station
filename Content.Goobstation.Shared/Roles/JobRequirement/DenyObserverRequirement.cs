using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Players;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

/// <summary>
/// Checks if the player joined as an observer through the lobby.
/// This is tracked via the ObserverStatusManager on the client.
/// Admins bypass this restriction.
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class DenyObserverRequirement : JobRequirement
{
    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        var observerManager = IoCManager.Resolve<IObserverStatusManager>();

        if (observerManager.IsAdmin)
            return true;

        var joinedAsObserver = observerManager.JoinedAsObserver;

        if (!Inverted)
        {
            if (!joinedAsObserver)
                return true;

            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-deny-observer"));
            return false;
        }

        if (joinedAsObserver)
            return true;

        reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-require-observer"));
        return false;
    }
}
