// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Speech;

/// <summary>
/// When present on a speaker, decorates their spoken message content.
/// Intended to be applied by the Security gas mask while equipped.
/// </summary>
[RegisterComponent]
public sealed partial class SecurityGasMaskAccentComponent : Component;
