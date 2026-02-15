using Content.Shared._CorvaxGoob.AppearanceConverter;
using Robust.Shared.GameStates;

namespace Content.Client._CorvaxGoob.AppearanceConverter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AppearanceConverterComponent : SharedAppearanceConverterComponent
{
    [DataField, AutoNetworkedField]
    public Dictionary<string, AppearanceConverterVisualTransformProfile> ProfilesVisualData = new();

    [DataField, AutoNetworkedField]
    public string? SelectedProfile = null;

    [DataField, AutoNetworkedField]
    public bool Transformed = false;

    [DataField, AutoNetworkedField]
    public TimeSpan NextTransformTime = TimeSpan.Zero;
}
