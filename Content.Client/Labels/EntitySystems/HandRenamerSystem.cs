using Content.Client.Labels.RenameSystem;
using Content.Client.Labels.UI;
using Content.Shared.HandRenamer;
using Content.Shared.Labels.EntitySystems;

namespace Content.Client.Labels.EntitySystems;

public sealed class HandRenamerSystem : SharedHandRenamerSystem
{
    protected override void UpdateUI(Entity<HandRenamerComponent> ent)
    {
        if (UserInterfaceSystem.TryGetOpenUi(ent.Owner, HandRenamerUiKey.Key, out var bui)
            && bui is HandRenamerBoundUserInterface cBui)
        {
            cBui.Reload();
        }
    }

}
