using Content.Shared.Disease;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Server.Disease;

public sealed partial class DiseaseSystem : SharedDiseaseSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    #region public API

    /// <summary>
    /// Tries to cure the entity of the given disease entity
    /// </summary>
    public override bool TryCure(EntityUid uid, EntityUid disease, DiseaseCarrierComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (comp.Diseases.Remove(disease))
            QueueDel(disease);
        else
            return false;

        Dirty(uid, comp);
        return true;
    }

    /// <summary>
    /// Tries to infect the entity with a given disease prototype
    /// </summary>
    public override bool TryInfect(EntityUid uid, EntProtoId diseaseId, [NotNullWhen(true)] out EntityUid? disease, DiseaseCarrierComponent? comp = null, bool force = false)
    {
        disease = null;
        if (!Resolve(uid, ref comp, false))
            return false;

        var spawned = Spawn(diseaseId, new EntityCoordinates(uid, Vector2.Zero));
        if (!TryInfect(uid, spawned, comp, force))
        {
            QueueDel(spawned);
            return false;
        }
        disease = spawned;
        return true;
    }

    #endregion

}
