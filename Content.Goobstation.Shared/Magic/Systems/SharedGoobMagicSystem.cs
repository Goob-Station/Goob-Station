using Content.Goobstation.Shared.TouchSpell;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Chat;
using Content.Shared.Popups;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.Magic.Systems;

public sealed partial class SharedGoobMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicActionComponent, ActionUpdateEvent>(OnActionUpdate);
        SubscribeLocalEvent<MagicActionComponent, ActionAttemptEvent>(OnActionAttempt, before: [typeof(TouchSpellSystem)]);
        SubscribeLocalEvent<MagicActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnActionUpdate(Entity<MagicActionComponent> ent, ref ActionUpdateEvent args)
    {
        if (!TryComp<ActionComponent>(args.Action, out var ac) || !ac.Container.HasValue)
            return;

        args.QueueDisable = !CanInvoke(ent, ac.Container.Value, out _);
    }

    private void OnActionAttempt(Entity<MagicActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (!CanInvoke(ent, args.User, out var reason))
        {
            args.Cancelled = true;

            if (reason != null)
                _popup.PopupEntity(reason, args.User, args.User, PopupType.SmallCaution);

            return;
        }
    }

    private void OnActionPerformed(Entity<MagicActionComponent> ent, ref ActionPerformedEvent args)
    {
        if (ent.Comp.InvocationLoc.HasValue)
            _chat.TrySendInGameICMessage(args.Performer, ent.Comp.InvocationLoc.Value, ent.Comp.InvocationType, false);
    }

    public bool CanInvoke(Entity<MagicActionComponent> ent, EntityUid performer, [NotNullWhen(false)] out string? invalidReason)
    {
        invalidReason = null;

        if (ent.Comp.Requirements == null)
            return true;

        foreach (var req in ent.Comp.Requirements)
        {
            if (!req.Valid(performer, EntityManager, out var reason))
            {
                invalidReason = reason;
                return false;
            }
        }
        return true;
    }
}
