using Content.Shared.Actions;
using Content.Shared.Backmen.Eye.NightVision.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Backmen.Eye.NightVision.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(NightVisionSystem))]
public sealed partial class NightVisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("isOn"), AutoNetworkedField]
    public bool IsNightVision;

    [DataField("color")]
    public Color NightVisionColor = Color.Green;

    [DataField]
    public bool IsToggle = false;

    [DataField] public EntityUid? ActionContainer;

    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public bool DrawShadows = false;

    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public bool GraceFrame = false;

    [DataField("playSoundOn")]
    public bool PlaySoundOn = true;
    public SoundSpecifier OnOffSound = new SoundPathSpecifier("/Audio/Backmen/Misc/night-vision-sound-effect_E_minor.ogg");
}

public sealed partial class NVInstantActionEvent : InstantActionEvent { }
