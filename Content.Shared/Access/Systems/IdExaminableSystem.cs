// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Overlays;
using Content.Shared.PDA;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Access.Systems;

public sealed class IdExaminableSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!; // Goobstation-WantedMenu
    [Dependency] private readonly AccessReaderSystem _accessReader = default!; // Goobstation-WantedMenu
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!; // Pirate-PsionicsMenu
    private const string MantisAccess = "Mantis"; // Pirate-PsionicsMenu
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IdExaminableComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);

        SubscribeLocalEvent<IdExaminableComponent, GetVerbsEvent<AlternativeVerb>>(OnWantedMenuOpen); // Goobstation-WantedMenu
    }

    private void OnGetExamineVerbs(EntityUid uid, IdExaminableComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        var detailsRange = _examineSystem.IsInDetailsRange(args.User, uid);
        var info = GetMessage(uid);

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = FormattedMessage.FromMarkupOrThrow(info);

                _examineSystem.SendExamineTooltip(args.User, uid, markup, true, false);
            },
            Text = Loc.GetString("id-examinable-component-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("id-examinable-component-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/character.svg.192dpi.png")),
        };
        args.Verbs.Add(verb);

        // Goobstation-WantedMenu-Start
        if (CanAccessWantedMenu(args.User, uid))
        {
            var wantedVerb = new ExamineVerb()
            {
                Act = () => OpenWantedUI(args.User, uid),
                Text = Loc.GetString("criminal-verb-name"),
                Category = VerbCategory.Examine,
                Disabled = !detailsRange,
                Message = detailsRange ? null : Loc.GetString("id-examinable-component-verb-disabled"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/_Goobstation/Interface/VerbIcons/wanted.png")),
                Priority = 1,
            };
            args.Verbs.Add(wantedVerb);
        }
        // Goobstation-WantedMenu-End

	        // Pirate-PsionicsMenu-Start
	        if (CanAccessPsionicsMenu(args.User, uid))
	        {
	            var psionicsVerb = new ExamineVerb()
	            {
	                Act = () => OpenPsionicsUI(args.User, uid),
	                Text = Loc.GetString("psionics-verb-name"),
	                Category = VerbCategory.Examine,
	                Disabled = !detailsRange,
	                Message = detailsRange ? null : Loc.GetString("id-examinable-component-verb-disabled"),
	                Icon = new SpriteSpecifier.Rsi(new("_EinsteinEngines/Icons/psi.rsi"), "psi"),
	                Priority = 2,
	            };
	            args.Verbs.Add(psionicsVerb);
	        }
	        // Pirate-PsionicsMenu-End
    }

    private void OnWantedMenuOpen(EntityUid uid,
            IdExaminableComponent comp,
            GetVerbsEvent<AlternativeVerb> args) // Goobstation-WantedMenu; Alternate activate in world hotkey
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        // Goobstation-WantedMenu-Start
        if (CanAccessWantedMenu(args.User, uid))
        {
            args.Verbs.Add(new AlternativeVerb()
            {
                Act = () => OpenWantedUI(args.User, uid),
                Text = Loc.GetString("criminal-verb-name"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/_Goobstation/Interface/VerbIcons/wanted.png")),
                Priority = 3
            });
        }

	        // Pirate-PsionicsMenu-Start
	        if (CanAccessPsionicsMenu(args.User, uid))
	        {
	            args.Verbs.Add(new AlternativeVerb()
	            {
	                Act = () => OpenPsionicsUI(args.User, uid),
	                Text = Loc.GetString("psionics-verb-name"),
	                Icon = new SpriteSpecifier.Rsi(new("_EinsteinEngines/Icons/psi.rsi"), "psi"),
	                Priority = 4
	            });
	        }
	        // Pirate-PsionicsMenu-End
    }

    private bool CanAccessWantedMenu(EntityUid user, EntityUid target) // Goobstation-WantedMenu
    {
        if (!_inventorySystem.TryGetSlotEntity(user, "eyes", out var eyes)
            || !TryComp<ShowCriminalRecordIconsComponent>(eyes, out _))
            return false;

        if (TryComp<AccessReaderComponent>(target, out var accessReader))
        {
            if (!_accessReader.IsAllowed(user, target, accessReader))
                return false;
        }

        return true;
    }

    private void OpenWantedUI(EntityUid uid, EntityUid target) // Goobstation-WantedMenu
    {
        _ui.TryToggleUi(target, SetWantedVerbMenu.Key, uid);
    }

    // Pirate-PsionicsMenu-Start
    private bool CanAccessPsionicsMenu(EntityUid user, EntityUid target)
    {
        // Check if user has psionics HUD glasses
        if (!_inventorySystem.TryGetSlotEntity(user, "eyes", out var eyes)
            || !TryComp<ShowPsionicsRecordIconsComponent>(eyes, out _))
            return false;

        // Check if target has an ID card (in inventory, hands, or is itself an ID card)
        if (!_idCardSystem.TryFindIdCard(target, out var _))
            return false;

        // Check if user has an ID card with Mantis access
        if (!_idCardSystem.TryFindIdCard(user, out var userIdCard))
            return false;

        if (!TryComp<AccessComponent>(userIdCard, out var userAccess))
            return false;

        // Check if user has Mantis access
        if (!userAccess.Tags.Contains(MantisAccess))
            return false;

        return true;
    }

    private void OpenPsionicsUI(EntityUid uid, EntityUid target)
    {
        _ui.TryToggleUi(target, SetPsionicsVerbMenu.Key, uid);
    }
    // Pirate-PsionicsMenu-End

    public string GetMessage(EntityUid uid)
    {
        return GetInfo(uid) ?? Loc.GetString("id-examinable-component-verb-no-id");
    }

    public string? GetInfo(EntityUid uid)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
        {
            // PDA
            if (TryComp(idUid, out PdaComponent? pda) &&
                TryComp<IdCardComponent>(pda.ContainedId, out var id))
            {
                return GetNameAndJob(id);
            }
            // ID Card
            if (TryComp(idUid, out id))
            {
                return GetNameAndJob(id);
            }
        }
        return null;
    }

    private string GetNameAndJob(IdCardComponent id)
    {
        var jobSuffix = string.IsNullOrWhiteSpace(id.LocalizedJobTitle) ? string.Empty : $" ({id.LocalizedJobTitle})";

        var val = string.IsNullOrWhiteSpace(id.FullName)
            ? Loc.GetString(id.NameLocId,
                ("jobSuffix", jobSuffix))
            : Loc.GetString(id.FullNameLocId,
                ("fullName", id.FullName),
                ("jobSuffix", jobSuffix));

        return val;
    }
}
