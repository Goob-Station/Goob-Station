using Content.Goobstation.Shared.HellGoose.Components;
using Content.Goobstation.Shared.HellGoose.Events;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.HellGoose.Systems;

public sealed class HellGooseStatueSharedSystem : EntitySystem
{
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
            ClientExclusive = true,
            Act = () =>
            {
                RaiseNetworkEvent(new PrayAtHellGooseStatueEvent());
            }
        };

        args.Verbs.Add(verb);
    }
}
