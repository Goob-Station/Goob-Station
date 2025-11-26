// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SlaughterDemon;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SlaughterDemonComponent : Component
{
    /// <summary>
    /// The list of mobs that the entity has devoured/consumed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> ConsumedMobs { get; set; } = new();

    /// <summary>
    /// The number of devoured mobs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Devoured;

    /// <summary>
    /// The walk modifier the entity gets once it stands on blood.
    /// </summary>
    [DataField]
    public float SpeedModWalk = 3f;

    /// <summary>
    /// The speed modifier the entity gets once it stands on blood.
    /// </summary>
    [DataField]
    public float SpeedModRun = 3f;

    /// <summary>
    /// This indicates whether the entity exited blood crawl
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool ExitedBloodCrawl;

    /// <summary>
    /// The accumulator for when a Slaughter Demon exits blood crawl
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Accumulator = TimeSpan.Zero;

    /// <summary>
    /// How long the speed boost lasts after a Slaughter Demon exits blood crawl
    /// </summary>
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.FromSeconds(6f);

    /// <summary>
    /// The jaunt effect when entering the jaunt
    /// </summary>
    [DataField]
    public EntProtoId JauntEffect = "SlaughterDemonJauntEffect";

    /// <summary>
    /// The jaunt effect when exiting the jaunt
    /// </summary>
    [DataField]
    public EntProtoId JauntUpEffect = "SlaughterDemonJauntUpEffect";

    /// <summary>
    /// Is the demon a Lesser Demon?
    /// </summary>
    [DataField]
    public bool IsLesser;

    /// <summary>
    /// Is the demon the Laughter Demon?
    /// </summary>
    [DataField]
    public bool IsLaughter;

    /// <summary>
    /// Plays when a demon blood crawls.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier BloodCrawlSounds;

    [DataField]
    public float BloodCrawlSoundLookup = 10f;

    [DataField]
    public float BloodCrawlSoundChance = 0.25f;
}


