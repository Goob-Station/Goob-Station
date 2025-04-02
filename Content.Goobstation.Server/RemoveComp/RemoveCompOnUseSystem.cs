using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.RemoveComp;

public sealed partial class RemoveCompOnUseSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RemoveCompOnUseComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, RemoveCompOnUseComponent component, UseInHandEvent args)
    {
        foreach (var comp in component.Components)
        {
            _entityManager.RemoveComponentDeferred(args.User,
                _entityManager.ComponentFactory.GetRegistration(comp.Value.Component).Type);
        }

        if (component.DisplayTextOnUse)
            _popupSystem.PopupEntity(Loc.GetString(component.PopupText), args.User, args.User);
        if (component.PlaySoundOnUse)
            _audio.PlayPvs(component.SoundOnUse, args.User);
    }
}
