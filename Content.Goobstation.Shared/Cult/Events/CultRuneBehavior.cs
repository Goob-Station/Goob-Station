using Content.Shared.Damage;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cult;

// i lowkey can't be assed to use entity effects because it'd make things even less comprehensible
// goobstation coder try not to reinvent the wheel challenge (level impossible)
[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public abstract partial class CultRuneBehavior
{
    [DataField] public bool BypassValidation = false;

    [DataField] public int RequiredInvokers = 1;

    [DataField] public Color PulseColor = Color.Black;

    [DataField] public LocId InvokeLoc = string.Empty;

    [DataField(required: true)] public LocId InspectNameLoc;

    [DataField(required: true)] public LocId InspectDescLoc;

    [DataField] public DamageSpecifier? Damage;

    protected bool Initialized = false;

    /// <summary>
    ///     You initialize entity systems here.
    /// </summary>
    public virtual void Initialize(IEntityManager ent)
    {
        Initialized = true;
    }

    /// <summary>
    ///     Checks if the invocation is valid. Should be run at all times.
    /// </summary>
    /// <param name="invalidReason">Can be used for popups to show the player a reason of why the invocation failed.</param>
    /// <returns></returns>
    public virtual bool IsValid(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, out string invalidReason)
    {
        if (!Initialized) Initialize(ent);

        invalidReason = string.Empty;

        if (BypassValidation) return true;

        if (invokers.Count < RequiredInvokers)
        {
            invalidReason = Loc.GetString("rune-invoke-fail-invokers", ("n", RequiredInvokers - invokers.Count));
            return false;
        }

        return true;
    }

    public abstract void Invoke(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, EntityUid? owner = null);
}
