using Robust.Client.UserInterface;

namespace Content.Client._Shitcode.UserActions.Tabs;

[Virtual]
public class BaseTabControl : Control
{
    public virtual bool UpdateState() { return true; }
}
