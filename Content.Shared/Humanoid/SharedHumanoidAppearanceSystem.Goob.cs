using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Shared.Humanoid;

public abstract partial class SharedHumanoidAppearanceSystem
{
    /// <summary>
    ///     Clones a humanoid's appearance to a target mob like <see cref="CloneAppearance"/>, but the target
    ///     keeps its own species; skin/markings the species can't have are dropped.
    /// </summary>
    /// <param name="source">Source entity to fetch the original appearance from.</param>
    /// <param name="target">Target entity to apply the source entity's appearance to.</param>
    /// <param name="sourceHumanoid">Source entity's humanoid component.</param>
    /// <param name="targetHumanoid">Target entity's humanoid component.</param>
    public void CloneSpeciesAppearance(EntityUid source, EntityUid target, HumanoidAppearanceComponent? sourceHumanoid = null,
        HumanoidAppearanceComponent? targetHumanoid = null)
    {
        if (!Resolve(source, ref sourceHumanoid, false) || !Resolve(target, ref targetHumanoid, false))
            return;

        targetHumanoid.EyeColor = sourceHumanoid.EyeColor;
        targetHumanoid.Age = sourceHumanoid.Age;
        targetHumanoid.Height = sourceHumanoid.Height;
        targetHumanoid.Width = sourceHumanoid.Width;
        targetHumanoid.Gender = sourceHumanoid.Gender;
        SetSex(target, sourceHumanoid.Sex, false, targetHumanoid);

        SetSkinColor(target, sourceHumanoid.SkinColor, false, true, targetHumanoid);

        targetHumanoid.MarkingSet.Clear();
        foreach (var (_, list) in sourceHumanoid.MarkingSet.Markings)
            foreach (var marking in list)
                AddMarking(target, marking.MarkingId, marking.MarkingColors, false, humanoid: targetHumanoid);

        targetHumanoid.MarkingSet.EnsureSpecies(targetHumanoid.Species, targetHumanoid.SkinColor, _markingManager, _proto);
        targetHumanoid.MarkingSet.EnsureSexes(sourceHumanoid.Sex, _markingManager);
        targetHumanoid.MarkingSet.EnsureDefault(targetHumanoid.SkinColor, targetHumanoid.EyeColor, _markingManager);

        SetBarkVoice(target, sourceHumanoid.BarkVoice, targetHumanoid);

        if (TryComp<GrammarComponent>(target, out var grammar))
            _grammarSystem.SetGender((target, grammar), sourceHumanoid.Gender);

        _identity.QueueIdentityUpdate(target);
        Dirty(target, targetHumanoid);
    }
}
