using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.NebulaCrawl;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class NebulaCrawlComponent : Component
{
    [DataField]
    public EntProtoId NebulaCrawlAction = "ActionVoidwalkerNebulaCrawl";

    [DataField, AutoNetworkedField]
    public EntityUid? NebulaCrawlActionEntity;
}
