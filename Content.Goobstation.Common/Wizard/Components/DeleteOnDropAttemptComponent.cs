// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Wizard.Components;

/// <summary>
/// Entity with this component will be deleted if user attempts to drop it
/// </summary>
[RegisterComponent]
public sealed partial class DeleteOnDropAttemptComponent : Component;