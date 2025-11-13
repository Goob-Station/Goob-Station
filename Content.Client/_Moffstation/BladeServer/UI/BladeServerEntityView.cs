using Content.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Moffstation.BladeServer.UI;

public sealed partial class BladeServerEntityView : SpriteView, IEntityControl
{
    public EntityUid? UiEntity { get; private set; }

    public new void SetEntity(EntityUid? entity)
    {
        UiEntity = entity;
        base.SetEntity(entity);
    }
}
