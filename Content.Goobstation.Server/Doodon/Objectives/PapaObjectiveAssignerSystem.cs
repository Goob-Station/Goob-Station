using Content.Goobstation.Shared.Doodons;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Doodon.Objectives;

public sealed class PapaObjectiveAssignerSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float CheckInterval = 1.0f;
    private float _accum;

    private static readonly string[] PapaObjectives =
    {
        "PapaKeepWorkersAliveObjective",
        "PapaBuildBuildingsObjective",
    };

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accum += frameTime;
        if (_accum < CheckInterval)
            return;

        _accum = 0f;

        var query = EntityQueryEnumerator<PapaDoodonComponent>();
        while (query.MoveNext(out var papaUid, out _))
        {
            if (!_mind.TryGetMind(papaUid, out var mindId, out var mind))
                continue;

            // Add each objective if missing
            foreach (var proto in PapaObjectives)
            {
                if (MindAlreadyHasObjectivePrototype(mind, proto))
                    continue;

                if (!_mind.TryAddObjective(mindId, mind, proto))
                {
                    _popup.PopupEntity($"Papa objective assignment failed: {proto}", papaUid, papaUid);
                }
            }
        }
    }

    private bool MindAlreadyHasObjectivePrototype(MindComponent mind, string protoId)
    {
        foreach (var obj in mind.Objectives)
        {
            if (!TryComp<MetaDataComponent>(obj, out var meta))
                continue;

            if (meta.EntityPrototype?.ID == protoId)
                return true;
        }

        return false;
    }
}
