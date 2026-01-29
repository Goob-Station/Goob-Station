using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Doodon.Building;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DoodonBuilderComponent : Component
{
    // What can be built
    [DataField, AutoNetworkedField]
    public List<EntProtoId> Buildables = new();

    // -1 = nothing selected
    [DataField, AutoNetworkedField]
    public int SelectedIndex = -1;

    [DataField, AutoNetworkedField]
    public float MaxBuildRange = 2.0f;

    // IMPORTANT: this should be the spawned action entity uid for the *hidden placer* action
    // (ActionDoodonBuildPlaceHidden), so the client can start targeting with it.
    [DataField, AutoNetworkedField]
    public EntityUid? BuildAction;

    public EntProtoId? GetSelected()
    {
        if (Buildables.Count == 0)
            return null;

        if (SelectedIndex < 0 || SelectedIndex >= Buildables.Count)
            return null;

        return Buildables[SelectedIndex];
    }
}
