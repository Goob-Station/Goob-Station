using Content.Client.Labels.RenameSystem;
using Content.Shared.HandRenamer;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Network;

namespace Content.Shared.Labels.EntitySystems;

public abstract class SharedHandRenamerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UserInterfaceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandRenamerComponent, AfterInteractEvent>(AfterInteractOn);
        SubscribeLocalEvent<HandRenamerComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
        SubscribeLocalEvent<HandRenamerComponent, HandRenamerNameChangedMessage>(OnHandRenamerNameChanged);
        SubscribeLocalEvent<HandRenamerComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<HandRenamerComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnGetState(Entity<HandRenamerComponent> ent, ref ComponentGetState args)
    {
        args.State = new HandRenamerComponentState(ent.Comp.AssignedName, ent.Comp.MaxNameChars);
    }


    private void OnHandleState(Entity<HandRenamerComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not HandRenamerComponentState state)
            return;

        ent.Comp.MaxNameChars = state.MaxNameChars;

        if (ent.Comp.AssignedName == state.AssignedName)
            return;

        ent.Comp.AssignedName = state.AssignedName;
        UpdateUI(ent);
    }

    protected virtual void UpdateUI(Entity<HandRenamerComponent> ent)
    {
    }

    private void RenameEntity(EntityUid uid, HandRenamerComponent? handRenamer, EntityUid target, out string? result)
    {
        if (!Resolve(uid, ref handRenamer))
        {
            result = null;
            return;
        }

        if (handRenamer.AssignedName == string.Empty)
        {
            if (_netManager.IsServer)
                _metaSystem.SetEntityName(target, handRenamer.AssignedName);
            result = Loc.GetString("hand-renamer-empty-name");
            return;
        }

        if (_netManager.IsServer)
            _metaSystem.SetEntityName(target, handRenamer.AssignedName);
        result = Loc.GetString("hand-renamer-successfully-renamed");
    }

    private void OnUtilityVerb(EntityUid uid, HandRenamerComponent handRenamer, GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Target is not { Valid: true } target ||
            _whitelistSystem.IsWhitelistFail(handRenamer.Whitelist, target) || !args.CanAccess)
            return;

        var renamerText = Loc.GetString("hand-renamer-rename-text");

        var verb = new UtilityVerb()
        {
            Act = () =>
            {
                RenameEntity(uid, handRenamer, target, out var result);
                if (result != null)
                    _popupSystem.PopupClient(result, args.User, args.User);
            },
            Text = renamerText
        };

        args.Verbs.Add(verb);
    }

    private void AfterInteractOn(EntityUid uid, HandRenamerComponent handRenamer, AfterInteractEvent args)
    {
        if (args.Target is not { Valid: true } target ||
            _whitelistSystem.IsWhitelistFail(handRenamer.Whitelist, target) || !args.CanReach)
            return;

        RenameEntity(uid, handRenamer, target, out var result);
        if (result != null)
            _popupSystem.PopupClient(result, args.User, args.User);
    }

    private void OnHandRenamerNameChanged(EntityUid uid, HandRenamerComponent handRenamer, HandRenamerNameChangedMessage args)
    {
        var newName = args.Name.Trim();
        handRenamer.AssignedName = newName[..Math.Min(handRenamer.MaxNameChars, newName.Length)];
        UpdateUI((uid, handRenamer));
        Dirty(uid, handRenamer);
    }
}
