using Content.Server.Antag;
using Content.Server.EUI;
using Content.Server.Heretic.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Eui;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Messages;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Heretic.Ui;

public sealed class FeastOfOwlsEui(EntityUid heretic, EntityUid rune, IEntityManager entityManager) : BaseEui
{
    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not FeastOfOwlsMessage { Accepted: true })
        {
            Close();
            return;
        }

        if (!entityManager.EntityExists(heretic) ||
            !entityManager.TryGetComponent(heretic, out HereticComponent? component))
        {
            Close();
            return;
        }

        if (component.Ascended)
        {
            entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("heretic-ritual-fail-already-ascended"), heretic, heretic);
            Close();
            return;
        }

        if (!component.CanAscend)
        {
            entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("heretic-ritual-fail-cannot-ascend"), heretic, heretic);
            Close();
            return;
        }

        if (!entityManager.EntityExists(rune) || !entityManager.System<TransformSystem>()
                .InRange(heretic, rune, SharedInteractionSystem.InteractionRange))
        {
            entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("feast-of-owls-eui-far-away"), heretic, heretic);
            Close();
            return;
        }

        component.CanAscend = false;
        component.ChosenRitual = null;
        component.KnownRituals.Remove("FeastOfOwls");
        entityManager.Dirty(heretic, component);

        entityManager.System<HereticRitualSystem>().RitualSuccess(rune, heretic);
        var feast = entityManager.EnsureComponent<FeastOfOwlsComponent>(heretic);

        entityManager.System<AntagSelectionSystem>()
            .SendBriefing(heretic,
                Loc.GetString("feast-of-owls-eui-briefing"),
                Color.Red,
                feast.BriefingSoundIntense);

        Close();
    }
}
