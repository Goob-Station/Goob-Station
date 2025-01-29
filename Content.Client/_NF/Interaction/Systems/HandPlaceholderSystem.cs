using Content.Client._NF.Hands.UI;
using Content.Client.Items;
using Content.Shared._NF.Interaction.Components;
using Content.Shared._NF.Interaction.Systems;
using JetBrains.Annotations;

namespace Content.Client._NF.Interaction.Systems;

/// <summary>
/// Handles interactions with items that spawn HandPlaceholder items.
/// </summary>
[UsedImplicitly]
public sealed partial class HandPlaceholderSystem : SharedHandPlaceholderSystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<HandPlaceholderComponent>(_ => new HandPlaceholderStatus());
    }
}
