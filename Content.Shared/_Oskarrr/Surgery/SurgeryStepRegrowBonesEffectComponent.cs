// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.Surgery;

/// <summary>
/// Respawns anatomical bones in a boneless body part.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryStepRegrowBonesEffectComponent : Component;
