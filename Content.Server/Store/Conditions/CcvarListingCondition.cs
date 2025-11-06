
using System.Linq;
using Content.Shared.Store;
using Robust.Shared.Configuration;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Only allows a listing based on bool CVAR
/// </summary>
public sealed partial class CcvarListingCondition : ListingCondition
{
    /// <summary>
    /// name of cvar
    /// </summary>
    [DataField( required: true)]
    public string Name;

    /// <summary>
    /// inverts showing the listing
    /// </summary>
    [DataField]
    public bool Invert = true;

    /// <summary>
    /// shud this item be listed if cvar dont exist
    /// </summary>
    [DataField]
    public bool ReturnIfFail = false;

    public override bool Condition(ListingConditionArgs args)
    {
        var cfgManager = IoCManager.Resolve<IConfigurationManager>();

        if (!cfgManager.IsCVarRegistered(Name))
           return ReturnIfFail;

        if (Invert)
            return !cfgManager.GetCVar<bool>(Name);

        return cfgManager.GetCVar<bool>(Name);
    }
}
