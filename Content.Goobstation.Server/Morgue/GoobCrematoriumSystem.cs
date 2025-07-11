using Content.Goobstation.Common.Morgue;
using Content.Goobstation.Shared.CrematorImmune;
using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Morgue;
using Content.Server.Morgue.Components;
using Content.Shared.Access.Components;
using Content.Shared.Database;
using Content.Shared.Emag.Systems;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Morgue;

/// <summary>
/// An extension to <see cref="CrematoriumSystem"/> without intruding into it too much
/// </summary>
public sealed class GoobCrematoriumSystem : CommonGoobCrematoriumSystem
{
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrematoriumComponent, GotEmaggedEvent>(OnEmagged);
    }

    public override bool CanCremate(EntityUid ent)
    {
        if (HasComp<CrematoriumImmuneComponent>(ent))
            return false;

        if (!_mob.IsDead(ent))
            return false;

        if (TryComp<MindContainerComponent>(ent, out var mindCont) && mindCont.Mind != null)
            return false;

        return true;
    }

    public override void TryDeleteItems(EntityUid ent, EntityUid crematorium)
    {
        // Todo inv checks, this should blow up if there are any high risk items
        throw new NotImplementedException();
    }

    private void OnEmagged(Entity<CrematoriumComponent> ent, ref GotEmaggedEvent args)
    {
        // It's an important thing innit
        _adminLog.Add(LogType.Emag, LogImpact.Extreme, $"{Loc.GetString("crematorium-emagged", ("user", ToPrettyString(args.UserUid)))}");

        // TODO handle item deletion
    }
}
