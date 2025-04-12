// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Soup-Byte07 <135303377+Soup-Byte07@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Security;

/// <summary>
/// Status used in Criminal Records.
///
/// None - the default value
/// Suspected - the person is suspected of doing something illegal
/// Wanted - the person is being wanted by security
/// Detained - the person is detained by security
/// Paroled - the person is on parole
/// Discharged - the person has been released from prison
/// Search - the person needs to be searched
/// Perma - the person has been sentenced to permanent imprisonment
/// Dangerous - the person is highly dangerous and may resist arrest
/// </summary>
public enum SecurityStatus : byte
{
    None,
    Suspected,
    Wanted,
    Detained,
    Paroled,
    Discharged,
    Search,
    Perma,
    Dangerous
}