using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfMutationShopComponent : Component;

[Serializable, NetSerializable]
public enum MutationUiKey : byte
{
    Key
}
