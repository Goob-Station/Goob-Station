using Content.Goobstation.Shared.Virology;

namespace Content.Goobstation.Server.Virology;

[RegisterComponent]
[Access(typeof(VirologyServerSystem))]
public sealed partial class VirologyServerComponent : Component
{
    /// <summary>
    ///     List of disease information entries we have.
    /// </summary>
    [ViewVariables]
    public List<DiseaseInformation> DiseaseInfo = new();
}
