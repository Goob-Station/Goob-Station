using System.Linq;
using Content.Server.Chat.Managers;
using Content.Shared.Interaction;
using Content.Shared.InteractionVerbs;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server.InteractionVerbs;

public sealed class InteractionVerbsSystem : SharedInteractionVerbsSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedInteractionSystem _interactions = default!;

    private EntityQuery<OccluderComponent> _occluderQuery;

    public override void Initialize()
    {
        base.Initialize();
        _occluderQuery = GetEntityQuery<OccluderComponent>();
    }

    protected override void SendChatLog(string message, EntityUid source, Filter filter, InteractionPopupPrototype popup, bool clip)
    {
        if (filter.Count <= 0)
            return;

        var color = popup.LogColor ?? InferColor(popup.PopupType);
        var wrappedMessage = message; // TODO: custom chat wraps maybe?

        // Exclude entities who cannot directly see the target of the popup. TODO this may have a high performance cost - although whispers do the same.
        // We only do this if the popup has to be logged into chat since that has some gameplay implications.
        if (clip && popup.DoClipping)
            filter.RemoveWhereAttachedEntity(ent => !CanSee(ent, source, popup.VisibilityRange));

        if (filter.Count == 1)
            _chatManager.ChatMessageToOne(popup.LogChannel, message, wrappedMessage, source, false, filter.Recipients.First().Channel, color);
        else
            _chatManager.ChatMessageToManyFiltered(filter, popup.LogChannel, message, wrappedMessage, source, false, false, color);
    }

    private Color InferColor(PopupType popup) => popup switch
    {
        // These are all hardcoded on client-side, so we have to improvise
        PopupType.LargeCaution or PopupType.MediumCaution or PopupType.SmallCaution => Color.Red,
        PopupType.Medium or PopupType.Small => Color.LightGray,
        _ => Color.White
    };

    private bool CanSee(EntityUid source, EntityUid target, float maxRange)
    {
        // TODO: InRangeUnobstructed has a pretty high performance cost and is not intended to be used like that.
        // We should see if we can move this to client side later, aka make the client check if the target is visible for it.
        return _interactions.InRangeUnobstructed(
            source, target, maxRange,
            CollisionGroup.Opaque,
            uid => !_occluderQuery.TryComp(uid, out var occluder) || !occluder.Enabled, // We ignore all entities that do not occlude light
            false);
    }
}
