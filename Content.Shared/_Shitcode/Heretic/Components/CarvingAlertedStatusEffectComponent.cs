using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CarvingAlertedStatusEffectComponent : Component
{
    [DataField]
    public HashSet<NetCoordinates> Locations = new();

    public const string Id = "alertcarving";

    [DataField]
    public SoundSpecifier? TeleportSound = new SoundCollectionSpecifier("Curse");
}
