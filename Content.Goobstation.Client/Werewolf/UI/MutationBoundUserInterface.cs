using Content.Goobstation.Shared.Werewolf.UI;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Werewolf.UI;

public sealed class MutationBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MutationMenu? _menu;

    public MutationBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        var spriteSystem = EntMan.System<SpriteSystem>();
        var dependencies = IoCManager.Instance!;

        _menu = new MutationMenu(Owner, dependencies.Resolve<IPrototypeManager>(), spriteSystem);

        _menu.Populate();

        _menu.OnClose += Close;
        _menu.ClaimForm += OnClaimForm;
        _menu.OpenCentered();
    }

    private void OnClaimForm()
    {
        SendPredictedMessage(new ClosedMessage());
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
