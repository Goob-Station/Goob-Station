// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Events;

/// <summary>
/// Raised before adding a martial art's combos to available combos.
/// </summary>
[Serializable]
public sealed partial class MartialArtsAttemptEvent : CancellableEntityEventArgs
{
    public ProtoId<MartialArtPrototype> MartialArt;

    public MartialArtsAttemptEvent(ProtoId<MartialArtPrototype> martialArt)
    {
        MartialArt = martialArt;
    }
};
