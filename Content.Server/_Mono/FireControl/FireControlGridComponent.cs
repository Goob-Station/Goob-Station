// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission.

namespace Content.Server._Mono.FireControl;

[RegisterComponent]
public sealed partial class FireControlGridComponent : Component
{
    [ViewVariables]
    public EntityUid? ControllingServer = null;
}
