using Content.Server.Antag;
using Content.Server.EUI;
using Content.Server.GameTicking.Rules;
using Content.Server.Heretic.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Eui;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Messages;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;

namespace Content.Server._Shitcode.Heretic.Ui;

public sealed class FeastOfOwlsEui : BaseEui
{
    private readonly EntityUid _heretic;

    private readonly EntityUid _rune;

    private readonly IEntityManager _entityManager;

    public FeastOfOwlsEui(EntityUid heretic, EntityUid rune, IEntityManager entityManager)
    {
        _heretic = heretic;
        _rune = rune;
        _entityManager = entityManager;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not FeastOfOwlsMessage { Accepted: true })
        {
            Close();
            return;
        }

        if (!_entityManager.EntityExists(_heretic) ||
            !_entityManager.TryGetComponent(_heretic, out HereticComponent? component))
        {
            Close();
            return;
        }

        if (component.Ascended)
        {
            _entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("heretic-ritual-fail-already-ascended"), _heretic, _heretic);
            Close();
            return;
        }

        if (!component.CanAscend)
        {
            _entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("heretic-ritual-fail-cannot-ascend"), _heretic, _heretic);
            Close();
            return;
        }

        if (!_entityManager.EntityExists(_rune) || !_entityManager.System<TransformSystem>()
                .InRange(_heretic, _rune, SharedInteractionSystem.InteractionRange))
        {
            _entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("feast-of-owls-eui-far-away"), _heretic, _heretic);
            Close();
            return;
        }

        component.CanAscend = false;
        component.ChosenRitual = null;
        component.KnownRituals.Remove("FeastOfOwls");
        _entityManager.Dirty(_heretic, component);

        _entityManager.System<HereticRitualSystem>().RitualSuccess(_rune, _heretic);

        _entityManager.System<AntagSelectionSystem>()
            .SendBriefing(_heretic,
                Loc.GetString("feast-of-owls-eui-briefing"),
                Color.Red,
                HereticRuleSystem.BriefingSoundIntense);

        _entityManager.EnsureComponent<FeastOfOwlsComponent>(_heretic);
        Close();
    }
}
