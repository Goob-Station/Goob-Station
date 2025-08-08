using Content.Shared.Damage;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfGutComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist;
}
