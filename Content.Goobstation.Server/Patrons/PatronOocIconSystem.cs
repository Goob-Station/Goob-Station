using Content.Goobstation.Shared.Patrons;
using Content.Server._RMC14.LinkAccount;

namespace Content.Goobstation.Server.Patrons;

public sealed partial class PatronOocIconSystem : EntitySystem
{
    [Dependency] private LinkAccountManager _linkAccount = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ChangePatronOocIconEvent>(OnChangeIcon);
    }

    private void OnChangeIcon(ChangePatronOocIconEvent ev, EntitySessionEventArgs args)
    {
        _linkAccount.SetOocIcon(args.SenderSession.UserId, ev.Icon?.Id);
    }
}
