using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Cinematic;

public sealed partial class SharedCinematicSystem
{
    #region Timeline

    /// <summary>
    /// Adds the components that last for the whole cinematic.
    /// </summary>
    private void LoadTimeline(Entity<CinematicComponent> ent, CinematicPrototype timeline)
    {
        if (ent.Comp.RegistryApplied)
            return;

        ent.Comp.RegistryApplied = true;

        if (timeline.AddComp is { } components)
            EntityManager.AddComponents(ent, components);
    }

    private void UnloadTimeline(Entity<CinematicComponent> ent, CinematicPrototype timeline)
    {
        if (timeline.AddComp is { } components)
            EntityManager.RemoveComponents(ent, components);
    }

    #endregion

    #region Segments

    private void HandleSegments(Entity<CinematicComponent> ent, CinematicPrototype timeline)
    {
        var index = GetSegmentIndex(ent.Comp, timeline);
        if (index == ent.Comp.ActiveSegment)
            return;

        UnloadSegment(ent, timeline);
        ent.Comp.ActiveSegment = index;
        LoadSegment(ent, timeline);
    }

    private void LoadSegment(Entity<CinematicComponent> ent, CinematicPrototype timeline)
    {
        if (!TryGetSegment(timeline, ent.Comp.ActiveSegment, out var segment))
            return;

        AddSegmentComponents(ent, timeline, segment);

        if (ent.Comp.Engaged)
            PlaySegmentSound(ent, segment.Sound);

        if (_net.IsServer || ent.Comp.Engaged)
            RaiseSegmentEvents(ent, segment);
    }

    private void UnloadSegment(Entity<CinematicComponent> ent, CinematicPrototype timeline)
    {
        if (TryGetSegment(timeline, ent.Comp.ActiveSegment, out var segment))
            RemoveSegmentComponents(ent, segment);
    }

    private void AddSegmentComponents(Entity<CinematicComponent> ent, CinematicPrototype timeline, CinematicSegment segment)
    {
        if (segment.AddComp is not { } components)
            return;

        if (timeline.AddComp is { } timelineComponents)
            foreach (var name in components.Keys)
                if (timelineComponents.TryGetValue(name, out var entry))
                    (ent.Comp.Overridden ??= new())[name] = entry;

        EntityManager.AddComponents(ent, components);
    }

    private void RemoveSegmentComponents(Entity<CinematicComponent> ent, CinematicSegment segment)
    {
        if (segment.AddComp is { } components)
            EntityManager.RemoveComponents(ent, components);

        if (ent.Comp.Overridden is not { } overridden)
            return;

        EntityManager.AddComponents(ent, overridden);
        overridden.Clear();
    }

    private void PlaySegmentSound(Entity<CinematicComponent> ent, SoundSpecifier? sound)
    {
        if (sound == null || _audio.PlayGlobal(sound, Filter.Local(), false)?.Entity is not { } stream)
            return;

        ent.Comp.SegmentSounds.Add(stream);
        EnsureComp<CinematicSceneSoundComponent>(stream);
    }

    private void RaiseSegmentEvents(Entity<CinematicComponent> ent, CinematicSegment segment)
    {
        foreach (var ev in segment.Events)
            RaiseLocalEvent(ent, _serialization.CreateCopy<object>(ev, notNullableOverride: true), true);
    }

    #endregion

    #region Lookup

    private static bool TryGetSegment(CinematicPrototype timeline, int index, [NotNullWhen(true)] out CinematicSegment? segment)
    {
        segment = null;

        if (index < 0 || index >= timeline.Segments.Count)
            return false;

        segment = timeline.Segments[index];
        return true;
    }

    private int GetSegmentIndex(CinematicComponent comp, CinematicPrototype timeline)
    {
        var elapsed = (float) (_timing.CurTime - comp.StartTime).TotalSeconds;
        if (elapsed < 0f)
            return -1;

        for (var i = 0; i < timeline.Segments.Count; i++)
        {
            elapsed -= timeline.Segments[i].Duration;
            if (elapsed < 0f)
                return i;
        }

        return -1;
    }

    #endregion
}
