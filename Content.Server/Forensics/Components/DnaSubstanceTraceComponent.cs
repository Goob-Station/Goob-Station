// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Forensics;

/// <summary>
/// This component stops the entity from leaving finger prints,
/// usually so fibres can be left instead.
/// </summary>
[RegisterComponent]
public sealed partial class DnaSubstanceTraceComponent : Component
{ }