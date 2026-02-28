using System;

namespace Content.Goobstation.Shared.Grab;

public interface IGrabCooldownComponent
{
    TimeSpan GrabCooldownDuration { get; set; }
    TimeSpan GrabCooldownEnd { get; set; }
    string GrabCooldownVerb { get; set; }
    bool IsCooldownActive(TimeSpan now);
    void StartCooldown(TimeSpan now);
}
