using System.Diagnostics.CodeAnalysis;
using Content.Corvax.Interfaces.Shared;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// Only sponsor that have it can select this loadout
/// </summary>
public sealed partial class SponsorLoadoutEffect : LoadoutEffect
{
    public override bool Validate(HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        LoadoutPrototype proto, // Corvax-Sponsors
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (session == null)
            return true;

        var sponsorProtos = GetPrototypes(session, collection);
        if (!sponsorProtos.Contains(proto.ID))
        {
            reason = FormattedMessage.FromMarkupOrThrow(Loc.GetString("loadout-sponsor-only"));
            return false;
        }

        return true;
    }

    public List<string> GetPrototypes(ICommonSession session, IDependencyCollection collection)
    {
        if (!collection.TryResolveType<ISharedSponsorsManager>(out var sponsorsManager))
            return new List<string>();

        var net = collection.Resolve<INetManager>();

        if (net.IsClient)
            return sponsorsManager.GetClientPrototypes();

        sponsorsManager.TryGetServerPrototypes(session.UserId, out var props);
        return props ?? [];
    }
}
