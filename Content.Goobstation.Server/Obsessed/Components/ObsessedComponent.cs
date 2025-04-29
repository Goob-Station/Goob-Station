// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Server.Obsessed;

[RegisterComponent]
public sealed partial class ObsessedComponent : Component
{
    public const float MaxSanity = 100f;

    /// <summary>
    /// Identifier of the obsession target
    /// </summary>
    /// <remarks>
    /// We are using integer to for proper photos work
    /// </remarks>
    [ViewVariables]
    public int Target = 0;

    public float Sanity
    {
        get => _sanity;
        set
        {
            _sanity = Math.Clamp(value, 0f, MaxSanity);
        }
    }

    /// <summary>
    /// Player sanity level. If it becomes too low, the obsession will become more hostile.
    /// </summary>
    [ViewVariables]
    private float _sanity = 100f;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How much sanity every interaction recovers.
    /// </summary>
    [DataField]
    public Dictionary<ObsessionInteraction, float> SanityRecovery = new()
    {
        { ObsessionInteraction.Hug, 2f },
        { ObsessionInteraction.Grab, 4f },
        { ObsessionInteraction.Follow, 1f },
        { ObsessionInteraction.PhotoTake, 10f },
        { ObsessionInteraction.PhotoLook, 5f },
        { ObsessionInteraction.Hear, 2f },
        { ObsessionInteraction.See, 1f }
    };

    /// <summary>
    /// How low sanity must be to trigger an effect.
    /// </summary>
    [DataField]
    public Dictionary<ObsessionEffect, float> SanityThresholds = new()
    {
        { ObsessionEffect.Popup, 75f },
        { ObsessionEffect.Sound, 70f },
        { ObsessionEffect.Shake, 50f },
        { ObsessionEffect.Speech, 35f },
        { ObsessionEffect.Damage, 10f }
    };
}
