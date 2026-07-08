// SPDX-License-Identifier: MIT

using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Light.Components;

/// <summary>
///     Device that allows user to quikly change bulbs in <see cref="PoweredLightComponent"/>
///     Can be reloaded by new light tubes or light bulbs
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedLightReplacerSystem))]
public sealed partial class LightReplacerComponent : Component
{
    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Weapons/click.ogg")
    {
        Params = new()
        {
            Volume = -4f
        }
    };

    /// <summary>
    /// Bulbs that were inserted inside light replacer
    /// </summary>
    [ViewVariables]
    public Container InsertedBulbs = default!;

    /// <summary>
    /// The default starting bulbs
    /// </summary>
    [DataField("contents")]
    public List<EntitySpawnEntry> Contents = new();

    /// <summary>
    /// Goobstation
    /// How much glass is inside of the light replacer.
    /// One means it will create a new bulb.
    /// </summary>
    [DataField]
    public float GlassRecycled;

    /// <summary>
    /// Goobstation
    /// How much glass required for one bulb.
    /// </summary>
    [DataField]
    public float GlassRequired = 1f;

    /// <summary>
    /// Goobstation
    /// How much glass given per bulb recycled.
    /// </summary>
    [DataField]
    public float GlassPerBulb = 0.25f;

    /// <summary>
    /// Goobstation
    /// What bulb is spawned when the max glass is reached?
    /// </summary>
    [DataField]
    public EntProtoId LightBulbProto = "LedLightTube";
}
