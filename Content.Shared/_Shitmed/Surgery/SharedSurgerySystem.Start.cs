using Content.Shared._Shitmed.CCVar;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared.Verbs;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;

    private EntityQuery<SurgeryTargetComponent> _targetQuery;

    private bool _noSelfOperate;

    private void InitializeStart()
    {
        _targetQuery = GetEntityQuery<SurgeryTargetComponent>();

        SubscribeLocalEvent<SurgeryToolComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);

        // cvar is yes var is no, invert it
        Subs.CVar(_config, SurgeryCVars.CanOperateOnSelf, x => _noSelfOperate = !x, true);
    }

    private void AttemptStartSurgery(Entity<SurgeryToolComponent> ent, EntityUid user, EntityUid target)
    {
        if (!IsLyingDown(target, user))
            return;

        if (_noSelfOperate && user == target)
        {
            _popup.PopupClient(Loc.GetString("surgery-error-self-surgery"), user, user);
            return;
        }

        _ui.OpenUi(target, SurgeryUIKey.Key, user);
        RefreshUI(target);
    }

    private void OnUtilityVerb(Entity<SurgeryToolComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
    {
        var target = args.Target;
        if (!args.CanInteract
            || !args.CanAccess
            || !_targetQuery.HasComp(target))
            return;

        var user = args.User;

        var verb = new UtilityVerb()
        {
            Act = () => AttemptStartSurgery(ent, user, target),
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Specific/Medical/Surgery/scalpel.rsi"), "scalpel"),
            Text = Loc.GetString("surgery-verb-text"),
            Message = Loc.GetString("surgery-verb-message"),
            DoContactInteraction = true
        };

        args.Verbs.Add(verb);
    }
}
