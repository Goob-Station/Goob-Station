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

    private readonly Entity<HereticComponent> _mind;

    private readonly EntityUid _rune;

    private readonly IEntityManager _entityManager;

    public FeastOfOwlsEui(EntityUid heretic,
        Entity<HereticComponent> mind,
        EntityUid rune,
        IEntityManager entityManager)
    {
        _heretic = heretic;
        _rune = rune;
        _entityManager = entityManager;
        _mind = mind;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not FeastOfOwlsMessage { Accepted: true })
        {
            Close();
            return;
        }

        if (!_entityManager.EntityExists(_heretic))
        {
            Close();
            return;
        }

        if (_mind.Comp.Ascended)
        {
            _entityManager.System<PopupSystem>()
                .PopupEntity(Loc.GetString("heretic-ritual-fail-already-ascended"), _heretic, _heretic);
            Close();
            return;
        }

        if (!_mind.Comp.CanAscend)
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

        _mind.Comp.CanAscend = false;
        _mind.Comp.ChosenRitual = null;
        _mind.Comp.KnownRituals.Remove("FeastOfOwls");
        _entityManager.Dirty(_mind);

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
