// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 PunishedJoe <PunishedJoeseph@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NerveSystemComponent : Component
{
    /// <summary>
    /// Pain.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Pain = 0f;

    /// <summary>
    /// How much of typical wound pain can this nerve system hold?
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 SoftPainCap = 120f;

    /// <summary>
    /// How much Pain can this nerve system hold.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 PainCap = 200f;

    // Don't change, OR I will break your knees, filled up upon initialization.
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntityUid, NerveComponent> Nerves = new();

    // Don't add manually!! Use built-in functions.
    public Dictionary<string, PainMultiplier> Multipliers = new();
    public Dictionary<(EntityUid, string), PainModifier> Modifiers = new();

    public Dictionary<EntityUid, AudioComponent> PlayedPainSounds = new();
    public Dictionary<EntityUid, (SoundSpecifier, AudioParams?, TimeSpan)> PainSoundsToPlay = new();

    [DataField("lastThreshold"), ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 LastPainThreshold = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public PainThresholdTypes LastThresholdType = PainThresholdTypes.None;

    [DataField("thresholdUpdate")]
    public TimeSpan ThresholdUpdateTime = TimeSpan.FromSeconds(1.6f);

    [DataField("reactionTime")]
    public TimeSpan PainReactionTime = TimeSpan.FromSeconds(0.07f);

    [DataField("adrenalineTime")]
    public TimeSpan PainShockAdrenalineTime = TimeSpan.FromSeconds(30f);

    [DataField]
    public TimeSpan CritScreamsIntervalMin = TimeSpan.FromSeconds(13f);

    [DataField]
    public TimeSpan CritScreamsIntervalMax = TimeSpan.FromSeconds(32f);
    public TimeSpan UpdateTime;
    public TimeSpan ReactionUpdateTime;
    public TimeSpan NextCritScream;

    [DataField("painShockStun")]
    public TimeSpan PainShockStunTime = TimeSpan.FromSeconds(2f);

    [DataField("organDamageStun")]
    public TimeSpan OrganDamageStunTime = TimeSpan.FromSeconds(12f);

    [DataField]
    public SoundSpecifier PainRattles = new SoundCollectionSpecifier("PainRattles");

    [DataField]
    public Dictionary<Sex, SoundSpecifier> PainScreams = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("PainScreamsShortMale")
            {
                Params = AudioParams.Default.WithVariation(0.04f),
            }
        },
        {
            Sex.Female, new SoundCollectionSpecifier("PainScreamsShortFemale")
            {
                Params = AudioParams.Default.WithVariation(0.04f),
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("PainScreamsShortMale") // yeah
            {
                Params = AudioParams.Default.WithVariation(0.2f),
            }
        },
    };

    [DataField]
    public Dictionary<Sex, SoundSpecifier> AgonyScreams = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("AgonyScreamsMale")
            {
                Params = AudioParams.Default.WithVariation(0.04f),
            }
        },
        {
            Sex.Female, new SoundCollectionSpecifier("AgonyScreamsFemale")
            {
                Params = AudioParams.Default.WithVariation(0.04f),
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("AgonyScreamsMale") // yeah
            {
                Params = AudioParams.Default.WithVariation(0.2f),
            }
        },
    };

    [DataField]
    public Dictionary<Sex, SoundSpecifier> PainShockScreams = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("PainShockScreamsMale")
            {
                Params = AudioParams.Default.WithVariation(0.05f),
            }
        },
        {
            Sex.Female, new SoundCollectionSpecifier("PainShockScreamsFemale")
            {
                Params = AudioParams.Default.WithVariation(0.05f),
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("PainShockScreamsMale") // yeah
            {
                Params = AudioParams.Default.WithVariation(0.2f),
            }
        },
    };

    [DataField]
    public Dictionary<Sex, SoundSpecifier> CritWhimpers = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("CritWhimpersMale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Female, new SoundCollectionSpecifier("CritWhimpersFemale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("CritWhimpersMale") // yeah
            {
                Params = AudioParams.Default,
            }
        },
    };

    [DataField]
    public Dictionary<Sex, SoundSpecifier> PainShockWhimpers = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("PainShockWhimpersMale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Female, new SoundCollectionSpecifier("PainShockWhimpersFemale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("PainShockWhimpersMale") // yeah
            {
                Params = AudioParams.Default,
            }
        },
    };

    [DataField]
    public Dictionary<Sex, SoundSpecifier> OrganDestructionReflexSounds = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("OrganDamagePainedMale")
            {
                Params = AudioParams.Default,
            }
       },
       {
            Sex.Female, new SoundCollectionSpecifier("OrganDamagePainedFemale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("OrganDamagePainedMale")
            {
                Params = AudioParams.Default,
            }
        },
    };

    [DataField]
    public Dictionary<Sex, SoundSpecifier> OrganDamageWhimpersSounds = new()
    {
        {
            Sex.Male, new SoundCollectionSpecifier("OrganDamageWhimpersMale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Female, new SoundCollectionSpecifier("OrganDamageWhimpersFemale")
            {
                Params = AudioParams.Default,
            }
        },
        {
            Sex.Unsexed, new SoundCollectionSpecifier("OrganDamageWhimpersMale")
            {
                Params = AudioParams.Default,
            }
        },
    };

    [DataField("reflexThresholds"), ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<PainThresholdTypes, FixedPoint2> PainThresholds = new()
    {
        { PainThresholdTypes.PainFlinch, 10 },
        { PainThresholdTypes.Agony, 40 },
        // Just having 'PainFlinch' is lame, people scream for a few seconds before passing out / getting pain shocked, so I added agony.
        // A lot of screams (individual pain screams poll), for the funnies.
        { PainThresholdTypes.PainShock, 65 }, // real
        // usually appears after an explosion. or some ultra big damage output thing, you might survive, and most importantly, you will fall down in pain.
        // :troll:
        { PainThresholdTypes.PainShockAndAgony, 85 },
    };
}
