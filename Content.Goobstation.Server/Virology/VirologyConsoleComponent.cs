using Content.Goobstation.Shared.Virology;

namespace Content.Goobstation.Server.Virology;

[RegisterComponent]
[Access(typeof(VirologyConsoleSystem))]
public sealed partial class VirologyConsoleComponent : Component
{
    /// <summary>
    ///     List of disease information entries we have.
    /// </summary>
    [ViewVariables]
    public List<DiseaseInformation> DiseaseInfo = new();
}
