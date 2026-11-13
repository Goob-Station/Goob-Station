using Content.Shared._Goobstation.Wizard;
using Content.Shared.Actions;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Events;

public sealed partial class MindTransferenceEvent : EntityTargetActionEvent
{
    /// <summary>
    /// Sound to be played
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Component to be transferred to new body if there is any
    /// I don't know if this can be serialized to yaml even
    /// </summary>
    public HashSet<Type> Components = new()
    {
        typeof(RevolutionaryComponent),
        typeof(HeadRevolutionaryComponent),
        typeof(WizardComponent),
        typeof(ApprenticeComponent),
    };
}
