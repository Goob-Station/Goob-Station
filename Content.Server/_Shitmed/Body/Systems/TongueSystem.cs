using System.Text.RegularExpressions;
using Content.Server.Body.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Server.Speech.Muting;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Robust.Shared.Random;

namespace Content.Server._Shitmed.Body.Systems;

public sealed class TongueSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex TonguelessLower = new("[gdntke]+", RegexOptions.Compiled);
    private static readonly Regex TonguelessUpper = new("[GDNTKE]+", RegexOptions.Compiled);
    private static readonly string[] LowerReplacements = ["aa", "oo", "'"];
    private static readonly string[] UpperReplacements = ["AA", "OO", "'"];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TongueComponent, OrganEnableChangedEvent>(OnOrganChanged, after: [typeof(BodySystem)]);
        SubscribeLocalEvent<TongueComponent, OrganAddedToBodyEvent>(OnOrganChanged);
        SubscribeLocalEvent<TongueComponent, OrganRemovedFromBodyEvent>(OnRemovedFromBody);
        SubscribeLocalEvent<TonguelessComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<TonguelessComponent, EmoteEvent>(OnEmote,
            before: [typeof(VocalSystem), typeof(MumbleAccentSystem)]);
        SubscribeLocalEvent<TonguelessComponent, ScreamActionEvent>(OnScreamAction,
            before: [typeof(VocalSystem)], after: [typeof(MutingSystem)]);
    }

    private void OnAccent(Entity<TonguelessComponent> ent, ref AccentGetEvent args)
    {
        if (args.Message.StartsWith('*'))
            return;

        args.Message = TonguelessLower.Replace(args.Message, _random.Pick(LowerReplacements));
        args.Message = TonguelessUpper.Replace(args.Message, _random.Pick(UpperReplacements));
    }

    private void OnEmote(Entity<TonguelessComponent> ent, ref EmoteEvent args)
    {
        if (args.Emote.Category.HasFlag(EmoteCategory.Vocal))
            args.Handled = true;
    }

    private void OnScreamAction(EntityUid uid, TonguelessComponent component, ScreamActionEvent args)
    {
        args.Handled = true;
    }

    private void OnOrganChanged<TEvent>(Entity<TongueComponent> ent, ref TEvent args)
    {
        if (TryComp<OrganComponent>(ent, out var organ))
            UpdateTongueless(ent, organ.Body, organ.Enabled);
    }

    private void OnRemovedFromBody(Entity<TongueComponent> ent, ref OrganRemovedFromBodyEvent args)
    {
        UpdateTongueless(ent, args.OldBody, false);
    }

    private void UpdateTongueless(EntityUid tongue, EntityUid? bodyUid, bool enabled)
    {
        if (TerminatingOrDeleted(tongue)
            || bodyUid is not { Valid: true } body
            || TerminatingOrDeleted(body))
            return;

        if (enabled)
        {
            RemComp<TonguelessComponent>(body);
            return;
        }

        if (_body.GetBodyOrganEntityComps<TongueComponent>(body).Count > 1)
            return;

        EnsureComp<TonguelessComponent>(body);
    }
}
