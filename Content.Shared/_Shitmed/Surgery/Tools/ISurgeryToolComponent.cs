namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

public interface ISurgeryToolComponent
{
    [DataField]
    public string ToolName { get; }

    // Mostly intended for discardable or non-reusable tools.
    [DataField]
    public bool? Used { get; set; }

    /// <summary>
    /// GoobStation: Multiply the step's doafter by this value.
    /// This is per-type so you can have something that's a good scalpel but a bad retractor.
    /// </summary>
    [DataField]
    public float Speed { get; set; }
}
