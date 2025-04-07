using Content.Goobstation.Common.Stunnable;
using Content.Goobstation.Shared.Stunnable;

namespace Content.Goobstation.Shared.Stun;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedGoobStunSystem : EntitySystem
{
    [Dependency] private readonly ClothingModifyStunTimeSystem _modifySystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GetClothingStunModifierEvent>(HandleGetClothingStunModifier);
    }

    private void HandleGetClothingStunModifier(GetClothingStunModifierEvent ev)
    {
        Log.Info("Handling Get Clothing Stun Modify");
        ev.Modifier = _modifySystem.GetModifier(ev.Target);
    }
}
