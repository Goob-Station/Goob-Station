// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Baa <9057997+Baa14453@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Sirionaut <148076704+Sirionaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 sirionaut <sirionaut@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Nutrition;

/// <summary>
///     Raised directed at the consumer when attempting to ingest something.
/// </summary>
public sealed class IngestionAttemptEvent : CancellableEntityEventArgs
{
    /// <summary>
    ///     The equipment that is blocking consumption. Should only be non-null if the event was canceled.
    /// </summary>
    public EntityUid? Blocker = null;
}

/// <summary>
/// Raised directed at the food after finishing eating a food before it's deleted.
/// Cancel this if you want to do something special before a food is deleted.
/// </summary>
public sealed class BeforeFullyEatenEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// The person that ate the food.
    /// </summary>
    public EntityUid User;
}

/// <summary>
/// Raised directed at the food being sliced before it's deleted.
/// Cancel this if you want to do something special before a food is deleted.
/// </summary>
public sealed class BeforeFullySlicedEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// The person slicing the food.
    /// </summary>
    public EntityUid User;
}