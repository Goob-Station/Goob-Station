// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.MalfAI;

namespace Content.Goobstation.Client.Silicon.MalfAI;

/// <summary>
/// This literally only exists so that the GetVerb event is fired clientside.
/// Might be needed in the future for malf ai related effects but not rn.
/// </summary>
public sealed class MalfStationAISystem : SharedMalfStationAISystem;