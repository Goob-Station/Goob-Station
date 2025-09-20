using Content.Goobstation.Shared.HellGoose.Components;
using Content.Server.Pointing.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
public sealed class HellGooseStatueSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellGooseStatueComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, HellGooseStatueComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("hell-goose-statue-accept"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/pray.svg.png")),
            Act = () => HandleActivation(uid, comp, args.User)
        };
        args.Verbs.Add(verb);
    }

    private void HandleActivation(EntityUid uid, HellGooseStatueComponent comp, EntityUid user)
    {
        _polymorphSystem.PolymorphEntity(user, "HellGooseMorph");
    }
}