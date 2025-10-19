using Content.Goobstation.Shared.HellGoose.Components;
using Content.Shared.Verbs;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.HellGoose.Systems;

public abstract class HellGooseStatueSharedSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellGooseStatueComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, HellGooseStatueComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var verb = new AlternativeVerb()
        {
            Text = Loc.GetString("hell-goose-statue-accept"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/pray.svg.png")),
            Act = () =>
            {
                GibTheGoose(args.User, uid);
                _popupSystem.PopupClient(Loc.GetString("hell-goose-statue-not-worthy"), args.User, args.User);
            }
        };

        args.Verbs.Add(verb);
    }
    protected virtual void GibTheGoose(EntityUid uid, EntityUid statue) {}
}
