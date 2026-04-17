using Content.Shared.Chat;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Shared.Database;
using Content.Shared.Popups;
using Robust.Shared.IoC;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Server.SpeakFont;

public sealed partial class SpeakFontOverrideSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpeakFontOverrideComponent, InventoryRelayedEvent<TransformSpeakerFontEvent>>(OnFontEvent);
        SubscribeLocalEvent<SpeakFontOverrideComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnFontEvent(Entity<SpeakFontOverrideComponent> entity, ref InventoryRelayedEvent<TransformSpeakerFontEvent> args)
    {
        if (entity.Comp.Enabled)
        {
            args.Args.FontId = entity.Comp.FontId;
            args.Args.FontSize = entity.Comp.FontSize;
            args.Args.Color = entity.Comp.Color;
        }
    }

    private void OnGetVerbs(EntityUid entity, SpeakFontOverrideComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.Using.HasValue || !HasComp<SpeakFontOverrideComponent>(args.Target))
            return;
        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("speakfontoverride-toggle"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () => SwitchMode(entity, comp),
            Impact = LogImpact.Low
        };
        args.Verbs.Add(verb);
    }

    private void SwitchMode(EntityUid ent, SpeakFontOverrideComponent comp)
    {
        comp.Enabled = !comp.Enabled;
        _popupSystem.PopupEntity(comp.Enabled ? Loc.GetString("speakfontoverride-enabled") : Loc.GetString("speakfontoverride-disabled"), ent);
    }
}
