using Content.Shared.Labels.Components;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// A filter that requires entities to have the exact same label as a set string.
/// Entities without a label will always fail it.
/// Set labels using a hand labeler.
/// </summary>
public sealed partial class LabelFilter : AutomationFilter
{
    /// <summary>
    /// The label to require.
    /// </summary>
    [DataField(required: true)]
    public string Label = string.Empty;

    public override bool IsAllowed(EntityUid uid)
    {
        return EntMan.GetComponentOrNull<LabelComponent>(uid)?.CurrentLabel == Label;
    }
}
