using Content.Server.Heretic.EntitySystems;
using Content.Shared.Dataset;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Text;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualKnowledgeBehavior : RitualCustomBehavior
{
    // made static so that it doesn't regenerate itself each time
    private static HashSet<ProtoId<TagPrototype>> _requiredTags = new();
    private HashSet<ProtoId<TagPrototype>> _missingTags = new();
    private List<EntityUid> _toDelete = new();

    private IPrototypeManager _prot = default!;
    private IRobustRandom _rand = default!;
    private EntityLookupSystem _lookup = default!;
    private HereticSystem _heretic = default!;
    private TagSystem _tag = default!;

    [ValidatePrototypeId<DatasetPrototype>]
    public const string EligibleTagsDataset = "EligibleTags";

    // this is basically a ripoff from hereticritualsystem
    public override bool Execute(RitualData args, out string? outstr)
    {
        _prot = IoCManager.Resolve<IPrototypeManager>();
        _rand = IoCManager.Resolve<IRobustRandom>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        _tag = args.EntityManager.System<TagSystem>();

        outstr = null;

        // generate new set of tags
        var dataset = _prot.Index<DatasetPrototype>(EligibleTagsDataset);
        if (_requiredTags.Count == 0)
            for (int i = 0; i < 4; i++)
                _requiredTags.Add(_rand.Pick(dataset.Values));

        var lookup = _lookup.GetEntitiesInRange(args.Platform, .75f);

        _toDelete.Clear();
        _missingTags.Clear();
        _missingTags.UnionWith(_requiredTags);
        foreach (var look in lookup)
        {
            if (!args.EntityManager.TryGetComponent<TagComponent>(look, out var tags))
                continue;

            _missingTags.RemoveWhere(tag =>
            {
                if (_tag.HasTag(tags, tag))
                {
                    _toDelete.Add(look);
                    return true;
                }

                return false;
            });
        }

        if (_missingTags.Count > 0)
        {
            var missing = string.Join(", ", _missingTags);
            outstr = Loc.GetString("heretic-ritual-fail-items", ("itemlist", missing));
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // delete all and reset
        foreach (var ent in _toDelete)
            args.EntityManager.QueueDeleteEntity(ent);
        _toDelete.Clear();

        if (args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
            _heretic.UpdateKnowledge(args.Performer, hereticComp, 4);

        // reset tags
        _requiredTags.Clear();
    }
}
