using Content.Goobstation.Shared.Gangwars.Components;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Gangwars;

/// <summary>
/// Draws gang role icons (head / member / locker) on entities, colored with their gangs color.
/// </summary>
public sealed class GangIconsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangLeaderComponent, GetStatusIconsEvent>(OnLeaderGetIcons);
        SubscribeLocalEvent<GangMemberComponent, GetStatusIconsEvent>(OnMemberGetIcons);
        SubscribeLocalEvent<GangLockerComponent, GetStatusIconsEvent>(OnLockerGetIcons);
    }

    private void OnLeaderGetIcons(Entity<GangLeaderComponent> ent, ref GetStatusIconsEvent args)
    {
        if (!TryComp<GangMemberComponent>(ent.Owner, out var member) || member.Gang == null)
            return;

        args.StatusIcons.Add(MakeIcon(ent.Comp.StatusIcon, member.Gang.Value));
    }

    private void OnMemberGetIcons(Entity<GangMemberComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ent.Comp.Gang == null || HasComp<GangLeaderComponent>(ent.Owner))
            return;

        args.StatusIcons.Add(MakeIcon(ent.Comp.StatusIcon, ent.Comp.Gang.Value));
    }

    private void OnLockerGetIcons(Entity<GangLockerComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ent.Comp.GangColor == null)
            return;

        args.StatusIcons.Add(MakeIcon(ent.Comp.StatusIcon, ent.Comp.GangColor.Value, hideOnStealth: false));
    }

    private StatusIconData MakeIcon(ProtoId<FactionIconPrototype> protoId, Color color, bool hideOnStealth = true)
    {
        var proto = _prototype.Index(protoId);
        return new StatusIconData
        {
            Icon = proto.Icon,
            Color = color,
            ShowTo = proto.ShowTo,
            HideOnStealth = hideOnStealth,
        };
    }
}
