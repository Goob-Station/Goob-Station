// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using System.Text;
using System.Linq;
using Content.Shared.Examine;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Stacks;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticRitualSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly GhoulSystem _ghoul = default!;

    public SoundSpecifier RitualSuccessSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/castsummon.ogg");

    public HereticRitualPrototype GetRitual(ProtoId<HereticRitualPrototype>? id)
    {
        if (id == null) throw new ArgumentNullException();
        return _proto.Index<HereticRitualPrototype>(id);
    }

    /// <summary>
    ///     Try to perform a selected ritual
    /// </summary>
    /// <returns> If the ritual succeeded or not </returns>
    public bool TryDoRitual(EntityUid performer,
        EntityUid mind,
        HereticComponent heretic,
        EntityUid platform,
        ProtoId<HereticRitualPrototype> ritualId)
    {
        // here i'm introducing locals for basically everything
        // because if i access stuff directly shit is bound to break.
        // please don't access stuff directly from the prototypes or else shit will break.
        // regards

        var rit = _proto.Index(ritualId);

        List<EntityUid>? limited = null;
        var exists = false;

        if (rit.Limit > 0)
            limited = heretic.LimitedTransmutations.GetOrNew(ritualId, out exists);

        if (limited != null)
        {
            if (exists)
                limited.RemoveAll(x => !Exists(x));

            if (limited.Count >= rit.Limit)
            {
                _popup.PopupEntity(Loc.GetString("heretic-ritual-fail-limit"), platform, performer);
                return false;
            }
        }

        var lookup = _lookup.GetEntitiesInRange(platform, 1.5f);

        var missingList = new Dictionary<string, float>();
        var toDelete = new List<EntityUid>();
        var toSplit = new List<(Entity<StackComponent> ent, int amount)>();

        // check for all conditions
        // this is god awful but it is that it is
        var behaviors = rit.CustomBehaviors ?? new();
        var requiredTags = rit.RequiredTags?.ToDictionary(e => e.Key, e => e.Value) ?? new();

        foreach (var behavior in behaviors)
        {
            var ritData = new RitualData(performer, platform, (mind, heretic), ritualId, EntityManager, limited, rit.Limit);

            if (!behavior.Execute(ritData, out var missingStr))
            {
                if (missingStr != null)
                    _popup.PopupEntity(missingStr, platform, performer);
                return false;
            }
        }

        foreach (var look in lookup)
        {
            // check for matching tags
            foreach (var tag in requiredTags)
            {
                if (!TryComp<TagComponent>(look, out var tags) // no tags?
                || _container.IsEntityInContainer(look)) // using your own eyes for amber focus?
                    continue;

                var ltags = tags.Tags;

                if (ltags.Contains(tag.Key))
                {
                    TryComp(look, out StackComponent? stack);
                    var amount = stack == null ? 1 : Math.Min(stack.Count, requiredTags[tag.Key]);

                    requiredTags[tag.Key] -= amount;

                    // prevent deletion of more items than needed
                    if (requiredTags[tag.Key] >= 0)
                    {
                        if (stack == null || stack.Count <= amount)
                            toDelete.Add(look);
                        else
                            toSplit.Add(((look, stack), amount));
                    }
                }
            }
        }

        // add missing tags
        foreach (var tag in requiredTags)
            if (tag.Value > 0)
                missingList.Add(tag.Key, tag.Value);

        // are we missing anything?
        if (missingList.Count > 0)
        {
            // we are! notify the performer about that!
            var sb = new StringBuilder();
            for (int i = 0; i < missingList.Keys.Count; i++)
            {
                var key = missingList.Keys.ToList()[i];
                var missing = $"{key} x{missingList[key]}";

                // makes a nice, list, of, missing, items.
                if (i != missingList.Count - 1)
                    sb.Append($"{missing}, ");
                else sb.Append(missing);
            }

            _popup.PopupEntity(Loc.GetString("heretic-ritual-fail-items", ("itemlist", sb.ToString())), platform, performer);
            return false;
        }

        // yay! ritual successfull!

        // reset fields to their initial values
        // BECAUSE FOR SOME REASON IT DOESN'T FUCKING WORK OTHERWISE!!!

        // finalize all of the custom ones
        foreach (var behavior in behaviors)
        {
            var ritData = new RitualData(performer, platform, (mind, heretic), ritualId, EntityManager, limited, rit.Limit);
            behavior.Finalize(ritData);
        }

        // ya get some, ya lose some
        foreach (var ent in toDelete)
            QueueDel(ent);

        foreach (var ((ent, stack), amount) in toSplit)
        {
            _stack.SetCount(ent, stack.Count - amount, stack);
        }

        var ghoulQuery = GetEntityQuery<GhoulComponent>();

        // add stuff
        var output = rit.Output ?? new();
        foreach (var ent in output.Keys)
        {
            for (var i = 0; i < output[ent]; i++)
            {
                var spawned = Spawn(ent, Transform(platform).Coordinates);

                if (ghoulQuery.TryComp(spawned, out var ghoul))
                    _ghoul.SetBoundHeretic(spawned, performer);

                if (limited == null)
                    continue;

                limited.Add(spawned);
                if (limited.Count >= rit.Limit)
                    break;
            }
        }

        if (rit.OutputEvent != null)
            RaiseLocalEvent(mind, rit.OutputEvent, true);

        if (rit.OutputKnowledge != null)
            _heretic.TryAddKnowledge((mind, heretic),  rit.OutputKnowledge.Value, performer);

        return true;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticRitualRuneComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticRitualRuneComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<HereticRitualRuneComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<HereticRitualRuneComponent, HereticRitualMessage>(OnRitualChosenMessage);
    }

    private void OnInteract(Entity<HereticRitualRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!_heretic.TryGetHereticComponent(args.User, out var heretic, out var mind))
            return;

        if (heretic.KnownRituals.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ritual-norituals"), args.User, args.User);
            return;
        }

        _uiSystem.OpenUi(ent.Owner, HereticRitualRuneUiKey.Key, args.User);
    }

    private void OnRitualChosenMessage(Entity<HereticRitualRuneComponent> ent, ref HereticRitualMessage args)
    {
        var user = args.Actor;

        if (!_heretic.TryGetHereticComponent(user, out var heretic, out _))
            return;

        heretic.ChosenRitual = args.ProtoId;

        var ritualName = Loc.GetString(GetRitual(heretic.ChosenRitual).LocName);
        _popup.PopupEntity(Loc.GetString("heretic-ritual-switch", ("name", ritualName)), user, user);
    }

    private void OnInteractUsing(Entity<HereticRitualRuneComponent> ent, ref InteractUsingEvent args)
    {
        if (!_heretic.TryGetHereticComponent(args.User, out var heretic, out var mind))
            return;

        if (!HasComp<MansusGraspComponent>(args.Used))
            return;

        if (heretic.ChosenRitual == null)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ritual-noritual"), args.User, args.User);
            return;
        }

        var successAnimation = _proto.Index(heretic.ChosenRitual.Value).RuneSuccessAnimation;

        if (!TryDoRitual(args.User, mind, heretic, ent, heretic.ChosenRitual.Value))
            return;

        if (successAnimation)
            RitualSuccess(ent, args.User);
    }

    private void OnExamine(Entity<HereticRitualRuneComponent> ent, ref ExaminedEvent args)
    {
        if (!_heretic.TryGetHereticComponent(args.Examiner, out var h, out _))
            return;

        var ritual = h.ChosenRitual != null ? GetRitual(h.ChosenRitual).LocName : null;
        var name = ritual != null ? Loc.GetString(ritual) : "None";
        args.PushMarkup(Loc.GetString("heretic-ritualrune-examine", ("rit", name)));
    }

    public void RitualSuccess(EntityUid ent, EntityUid user)
    {
        _audio.PlayPvs(RitualSuccessSound, ent, AudioParams.Default.WithVolume(-3f));
        _popup.PopupEntity(Loc.GetString("heretic-ritual-success"), ent, user);
        Spawn("HereticRuneRitualAnimation", Transform(ent).Coordinates);
    }
}
