using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Common.Cloning;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Heretic;
using Content.Shared.Store.Components;
using Content.Shared.Chat;
using Content.Shared.Cloning;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Cloning;
/// <summary>
/// Handles adding and removing comps (like antags) we don't want, to remove duplication when cloning
/// We don't do these in clone.yml because we only want them to transfer / remove when the mind transfers
/// </summary>
public sealed partial class CloningSpecialCaseHandler : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreComponent, TransferredToCloneEvent>(OnTransferredToClone);
        SubscribeLocalEvent<HereticComponent, TransferredToCloneEvent>(OnTransferredToClone);
        SubscribeLocalEvent<ChangelingComponent, TransferredToCloneEvent>(OnTransferredToClone);
    }

    // Lotta things end up using store like ling and heretic so im handling both here.
    // We should move storecomp out of bodies fr
    #region StoreComponent
    private void OnTransferredToClone(Entity<StoreComponent> oldBody, ref TransferredToCloneEvent args)
    {
        CopyComp(oldBody.Owner, args.NewBody, oldBody.Comp);
        RemComp<StoreComponent>(oldBody.Owner);
    }
    #endregion

    #region Heretic
    private void OnTransferredToClone(Entity<HereticComponent> oldBody, ref TransferredToCloneEvent args)
    {
        CopyComp(oldBody.Owner, args.NewBody, oldBody.Comp);
        RemComp<HereticComponent>(oldBody.Owner);
    }
    #endregion

    #region Changeling

        #region PostCloning
    private static readonly string[] OriginalBodyGibbedMessages =
    {
        // Multiple, cause flavour
        "cloning-console-original-gibbed-1",
        "cloning-console-original-gibbed-2",
        "cloning-console-original-gibbed-3",
        "cloning-console-original-gibbed-4",
    };
    private void OnTransferredToClone(Entity<ChangelingComponent> oldBody, ref TransferredToCloneEvent args)
    {
        if (!TryComp<CloningPodComponent>(args.ClonePod, out var cloningPodComp))
            return;

        if (!TryComp<ChangelingIdentityComponent>(oldBody.Owner, out var originalChangelingIdentityComp))
            return;

        CopyComps(oldBody.Owner,args.NewBody,null, [
                oldBody.Comp,
                originalChangelingIdentityComp,
        ]);

        //kinda overkill to remove if we're gibbing but eh
        RemComp<ChangelingComponent>(oldBody);
        RemComp<ChangelingIdentityComponent>(oldBody);

        if (cloningPodComp.ConnectedConsole == null) // okay no message for you
        {
            _bodySystem.GibBody(oldBody);
            return;
        }

        GibOriginalBody(oldBody, cloningPodComp.ConnectedConsole.Value);
    }

    private void GibOriginalBody(EntityUid originalBody, EntityUid cloningConsoleEnt)
    {
        var gibMessage = _random.Pick(OriginalBodyGibbedMessages);
        _chatSystem.TrySendInGameICMessage(
            cloningConsoleEnt,
            Loc.GetString(gibMessage),
            InGameICChatType.Speak,
            false);

        _bodySystem.GibBody(originalBody);
    }
        #endregion PostCloning

    #endregion
}
