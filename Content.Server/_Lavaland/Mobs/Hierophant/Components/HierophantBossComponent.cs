// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantBossComponent : MegafaunaComponent
{
    /// <summary>
    /// Amount of time for one damaging tile to charge up and deal the damage to anyone above it.
    /// </summary>
    public const float TileDamageDelay = 0.6f;

    /// <summary>
    ///     Gets calculated automatically in the <see cref="HierophantSystem"/>.
    ///     Is responsive for how fast and strong hierophant attacks.
    /// </summary>
    [ViewVariables]
    public float CurrentAnger = 1f;

    /// <summary>
    /// Minimal amount of anger that Hierophant can have.
    /// Tends to 3 when health tends to 0.
    /// </summary>
    [DataField]
    public float MinAnger = 1f;

    /// <summary>
    /// Max cap for anger.
    /// </summary>
    [DataField]
    public float MaxAnger = 6f;

    [DataField]
    public float InterActionDelay = 1.5f * TileDamageDelay * 1000f;

    [DataField]
    public float AttackCooldown = 4.5f * TileDamageDelay;

    [ViewVariables]
    public float AttackTimer = 2.5f * TileDamageDelay;

    [DataField]
    public float MinAttackCooldown = 1f * TileDamageDelay;

    /// <summary>
    /// Amount of anger to adjust on a hit.
    /// </summary>
    [DataField]
    public float AdjustAngerOnAttack = 0.1f;

    /// <summary>
    /// Connected field generator, will try to teleport here when it's inactive.
    /// </summary>
    [ViewVariables]
    public EntityUid? ConnectedFieldGenerator;

    /// <summary>
    /// Controls
    /// </summary>
    [DataField]
    public Dictionary<HierophantAttackType, float> Attacks = new()
    {
        { HierophantAttackType.Chasers, 0.1f },
        { HierophantAttackType.Crosses, 0.1f },
        { HierophantAttackType.DamageArea, 0.2f },
        { HierophantAttackType.Blink, 0.2f },
    };

    /// <summary>
    /// Attack that was done previously, so we don't repeat it over and over.
    /// </summary>
    [DataField]
    public HierophantAttackType PreviousAttack;
}

public enum HierophantAttackType
{
    Invalid,
    Chasers,
    Crosses,
    DamageArea,
    Blink,
}