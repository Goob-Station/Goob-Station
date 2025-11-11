using Content.Goobstation.Shared.Shizophrenia;
using Robust.Server.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Shizophrenia;

/// <summary>
/// Entry for fake appearance hallucinations
/// </summary>
public sealed partial class AppearanceHallucinationsEntry : BaseHallucinationsEntry
{
    /// <summary>
    /// Appearance data that can be applied to entity
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<HallucinationAppearanceData> Appearances = new();

    protected override void Perform(EntityUid source, IEntityManager entMan, IRobustRandom random)
    {
        var player = IoCManager.Resolve<IPlayerManager>();
        if (!player.TryGetSessionByEntity(source, out var session))
            return;

        var selected = random.Pick(Appearances);
        entMan.EntityNetManager?.SendSystemNetworkMessage(new SetHallucinationAppearanceMessage(selected), session.Channel);
    }
}
