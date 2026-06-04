namespace Content.Shared.Abilities.Mime;

public sealed partial class MimePowersComponent
{
    /// <summary>
    /// Amount of times mime has tried to speak with a nonverbal language.
    /// </summary>
    [DataField]
    public int NonverbalViolationCount = 0;

    /// <summary>
    /// Amount of violations before mime is punished for trying to use nonverbal languages.
    /// </summary>
    [DataField]
    public int NonverbalMaxViolationCount = 3;

    /// <summary>
    /// smite.
    /// </summary>
    [DataField]
    public int NonverbalViolationCountYouAreDone = 10;
}