// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Obsessed;

[RegisterComponent]
public sealed partial class ObsessedComponent : Component
{
    public const float MaxSanity = 100f;

    /// <summary>
    /// Identifier of the obsession target
    /// </summary>
    /// <remarks>
    /// We are using int to for proper photos work
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    public int TargetId = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Target = EntityUid.Invalid;
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
    [ViewVariables(VVAccess.ReadWrite)]
    private float _sanity = 100f;

    [DataField]
    public float SanityLoss = -1.5f;

    /// <summary>
    /// How much sanity every interaction recovers.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<ObsessionInteraction, float> SanityRecovery = new()
    {
        { ObsessionInteraction.Hug, 2f },
        { ObsessionInteraction.Grab, 4f },
        { ObsessionInteraction.Follow, 2f },
        { ObsessionInteraction.PhotoTake, 10f },
        { ObsessionInteraction.PhotoLook, 5f },
        { ObsessionInteraction.Hear, 2f },
        { ObsessionInteraction.See, 1f }
    };

    /// <summary>
    /// How low sanity must be to trigger an effect.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<ObsessionEffect, float> SanityThresholds = new()
    {
        { ObsessionEffect.Popup, 75f },
        { ObsessionEffect.Sound, 70f },
        { ObsessionEffect.Shake, 50f },
        { ObsessionEffect.Speech, 35f },
        { ObsessionEffect.Damage, 10f }
    };

    /// <summary>
    /// Pack for low sanity popups
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<LocalizedDatasetPrototype> PopupDataset = "ObsessionPopups";

    /// <summary>
    /// Contains the last interactions with the target. Exsists to avoid hug/grab/other spam
    /// Does not contains seeing and following
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<ObsessionInteraction> LastInteractions = new();

    /// <summary>
    /// A counter for updates that we see the target
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int FollowUpdates = 0;

    /// <summary>
    /// How many updates we need to see the target to start recovering additional sanity
    /// </summary>
    [DataField]
    public int FollowUpdatesToRecovery = 10;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan SanityNextUpdate = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public float SanityUpdateInterval = 2f;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan EffectNextUpdate = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public (float, float) EffectUpdateInterval = (7f, 15f);
}
