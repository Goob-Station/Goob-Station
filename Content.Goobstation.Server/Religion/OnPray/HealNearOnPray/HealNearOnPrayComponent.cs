// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

[RegisterComponent]
public sealed partial class HealNearOnPrayComponent : Component
{
    [DataField]
    public DamageSpecifier Healing = new();

    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Which sound to play on heal.
    /// </summary>
    [DataField]
    public SoundSpecifier HealSoundPath = new SoundPathSpecifier("/Audio/Effects/holy.ogg");

    /// <summary>
    /// Which sound to play on damage.
    /// </summary>
    [DataField]
    public SoundSpecifier SizzleSoundPath = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    /// <summary>
    /// Which effect to display on heal.
    /// </summary>
    [DataField]
    public EntProtoId HealEffect = "EffectSpark";

    /// <summary>
    /// Which effect to display on damage.
    /// </summary>
    [DataField]
    public EntProtoId DamageEffect = "WeaponArcTempSlash";

    [DataField]
    public List<EntityUid> HealedEntities = new();

    [DataField]
    public int Range = 5;
}
