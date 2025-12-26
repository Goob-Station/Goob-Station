using Content.Shared.Atmos;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingStasisComponent : Component
{
    // protoIDs
    [DataField]
    public EntProtoId ActionId = "ActionRegenerativeStasis";

    [DataField]
    public ProtoId<DamageTypePrototype> AliveDamageProto = "Slash";

    [DataField]
    public ProtoId<DamageTypePrototype> CritDamageProto = "Asphyxiation";

    // actionsEnts
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEnt;

    // LocIds
    [DataField]
    public LocId EnterPopup = "changeling-stasis-enter";

    [DataField]
    public LocId EnterDamagedPopup = "changeling-stasis-enter-damaged";

    [DataField]
    public LocId EnterDeadPopup = "changeling-stasis-enter-dead";

    [DataField]
    public LocId ExitPopup = "changeling-stasis-exit";

    [DataField]
    public LocId ExitDefibPopup = "changeling-stasis-exit-defib";

    [DataField]
    public LocId SelfReviveFailPopup = "self-revive-fail";

    [DataField]
    public LocId AbsorbedPopup = "changeling-stasis-absorbed";

    [DataField]
    public LocId EnterAlivePopup = "suicide-command-default-text-others"; // suicide message

    // the important stuff
    [DataField, AutoNetworkedField]
    public TimeSpan DefaultStasisTime = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public TimeSpan CritStasisTime = TimeSpan.FromSeconds(45);

    [DataField, AutoNetworkedField]
    public TimeSpan DeadStasisTime = TimeSpan.FromSeconds(60);

    [DataField, AutoNetworkedField]
    public TimeSpan StasisTime = default!;

    [DataField, AutoNetworkedField]
    public bool IsInStasis;

    [DataField, AutoNetworkedField]
    public float IdealTemp = Atmospherics.T37C;

    [DataField, AutoNetworkedField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(1);
}
