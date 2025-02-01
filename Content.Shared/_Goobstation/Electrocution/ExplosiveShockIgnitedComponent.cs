using System;

namespace Content.Shared.Electrocution;

[RegisterComponent]
public sealed partial class ExplosiveShockIgnitedComponent : Component
{
    public TimeSpan ExplodeAt;
}
