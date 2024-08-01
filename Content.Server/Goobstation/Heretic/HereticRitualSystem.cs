using Content.Server.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using System.Text;

namespace Content.Server.Heretic;

public sealed partial class HereticRitualSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HereticKnowledgeSystem _knowledge = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public SoundSpecifier RitualSuccessSound = new SoundPathSpecifier("/Audio/Goobstation/Heretic/castsummon.ogg");

    public HereticRitualPrototype GetRitual(ProtoId<HereticRitualPrototype> id)
        => _proto.Index(id);

    public bool TryDoRitual(Entity<HereticComponent> performer, EntityUid platform, ProtoId<HereticRitualPrototype> ritualId)
    {
        var rit = GetRitual(ritualId);
        var lookup = _lookup.GetEntitiesInRange(platform, 2.5f);

        var missingList = new List<string>();
        var toDelete = new List<EntityUid>();

        // check for matching entity names
        if (rit.RequiredEntityNames != null)
        {
            foreach (var name in rit.RequiredEntityNames)
            {
                foreach (var look in lookup)
                {
                    if (!TryComp<MetaDataComponent>(look, out var metadata))
                        continue;

                    if (metadata.EntityName == name.Key)
                    {
                        rit.RequiredEntityNames[name.Key] -= 1;
                        toDelete.Add(look);
                    }
                }
            }

            foreach (var name in rit.RequiredEntityNames)
                if (name.Value > 0)
                    missingList.Add(name.Key);
        }

        // check for matching tags
        if (rit.RequiredTags != null)
        {
            foreach (var tag in rit.RequiredTags)
            {
                foreach (var look in lookup)
                {
                    if (!TryComp<TagComponent>(look, out var tags))
                        continue;

                    foreach (var ltag in tags.Tags)
                    {
                        if (ltag == tag.Key)
                        {
                            rit.RequiredTags[tag.Key] -= 1;
                            toDelete.Add(look);
                        }
                    }
                }
            }

            foreach (var tag in rit.RequiredTags)
                if (tag.Value > 0)
                    missingList.Add(tag.Key);
        }

        // are we missing anything?
        if (missingList.Count > 0)
        {
            // we are! notify the performer about that!
            var sb = new StringBuilder();
            foreach (var missing in missingList)
                sb.Append(missing);

            _popup.PopupEntity(Loc.GetString("heretic-ritual-fail-items", ("itemlist", sb.ToString())), performer, performer);
            return false;
        }

        // yay! ritual successfull!
        _audio.PlayPvs(RitualSuccessSound, platform, AudioParams.Default.WithVolume(-3f));

        // ya get some, ya lose some
        foreach (var ent in toDelete)
            QueueDel(ent);

        // add stuff
        if (rit.Output != null && rit.Output.Count > 0)
        {
            foreach (var ent in rit.Output.Keys)
                for (int i = 0; i < rit.Output[ent]; i++)
                    Spawn(ent, Transform(platform).Coordinates);
        }
        if (rit.OutputEvent != null)
            RaiseLocalEvent(rit.OutputEvent);

        if (rit.OutputKnowledge != null)
            _knowledge.AddKnowledge(performer, (ProtoId<HereticKnowledgePrototype>) rit.OutputKnowledge);

        return true;
    }
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticRitualRuneComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticRitualRuneComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteract(Entity<HereticRitualRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (heretic.KnownRituals.Count == 0)
        {

            return;
        }
        if (heretic.ChosenRitual != null)
        {
            var index = heretic.KnownRituals.FindIndex(m => m == heretic.ChosenRitual) + 1;

            if (index >= heretic.KnownRituals.Count)
                index = 0;

            heretic.ChosenRitual = heretic.KnownRituals[index];

            var ritualName = Loc.GetString(GetRitual(heretic.ChosenRitual.Value).Name);

            _popup.PopupEntity(Loc.GetString("heretic-ritual-switch", ("name", ritualName)), ent, ent);
        }
    }
    private void OnInteractUsing(Entity<HereticRitualRuneComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (!TryComp<MansusGraspComponent>(args.Used, out var grasp))
            return;

        if (heretic.ChosenRitual == null)
        {

            return;
        }

        if (!TryDoRitual(args.User, ent, (ProtoId<HereticRitualPrototype>) heretic.ChosenRitual))
        {

            return;
        }
    }
}
