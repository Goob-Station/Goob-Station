// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Projectiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ProjectileComponent : Component
{
    /// <summary>
    ///     The angle of the fired projectile.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle Angle;

    /// <summary>
    ///     The effect that appears when a projectile collides with an entity.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId? ImpactEffect;

    /// <summary>
    ///     User that shot this projectile.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Shooter;

    /// <summary>
    ///     Weapon used to shoot.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Weapon;

    /// <summary>
    ///     The projectile spawns inside the shooter most of the time, this prevents entities from shooting themselves.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreShooter = true;

    /// <summary>
    ///     The amount of damage the projectile will do.
    /// </summary>
    [DataField(required: true)] [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = new();

    /// <summary>
    ///     If the projectile should be deleted on collision.
    /// </summary>
    [DataField]
    public bool DeleteOnCollide = true;

    /// <summary>
    ///     Ignore all damage resistances the target has.
    /// </summary>
    [DataField]
    public bool IgnoreResistances = false;

    /// <summary>
    ///     Get that juicy FPS hit sound.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundHit;

    /// <summary>
    ///     Force the projectiles sound to play rather than potentially playing the entity's sound.
    /// </summary>
    [DataField]
    public bool ForceSound = false;

    /// <summary>
    ///     Whether this projectile will only collide with entities if it was shot from a gun (if <see cref="Weapon"/> is not null).
    /// </summary>
    [DataField]
    public bool OnlyCollideWhenShot = false;

    /// <summary>
    ///     If true, the projectile has hit enough targets and should no longer interact with further collisions pending deletion.
    /// </summary>
    [DataField]
    public bool ProjectileSpent;

    /// <summary>
    ///     When a projectile has this threshold set, it will continue to penetrate entities until the damage dealt reaches this threshold.
    /// </summary>
    [DataField]
    public FixedPoint2 PenetrationThreshold = 10f;

    /// <summary>
    ///     If set, the projectile will not penetrate objects that lack the ability to take these damage types.
    /// </summary>
    [DataField]
    public List<string>? PenetrationDamageTypeRequirement;

    /// <summary>
    ///     Tracks the amount of damage dealt for penetration purposes.
    /// </summary>
    [DataField]
    public FixedPoint2 PenetrationAmount = FixedPoint2.Zero;

    // Goobstation start
    [DataField]
    public float ArmorPenetration;

    [DataField]
    public bool Penetrate;

    /// <summary>
    ///     Collision mask of what not to penetrate if <see cref="Penetrate"/> is true.
    /// </summary>
    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int NoPenetrateMask = 0;

    [NonSerialized]
    public List<EntityUid> IgnoredEntities = new();
    // Goobstation end
}
