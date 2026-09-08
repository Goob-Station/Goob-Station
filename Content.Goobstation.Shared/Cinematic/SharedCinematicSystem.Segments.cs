using System.Diagnostics.CodeAnalysis;
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

        LoadSegmentComponents(ent, segment);
        LoadSegmentSound(ent, segment);
        RaiseSegmentEvents(ent, segment);
    }

    private void UnloadSegment(Entity<CinematicComponent> ent, CinematicPrototype timeline)
    {
        if (!TryGetSegment(timeline, ent.Comp.ActiveSegment, out var segment))
            return;

        UnloadSegmentComponents(ent, segment);
        ent.Comp.ActiveSegment = -1;
    }

    private void LoadSegmentComponents(Entity<CinematicComponent> ent, CinematicSegment segment)
    {
        if (segment.AddComp is { } components)
            EntityManager.AddComponents(ent, components);
    }

    private void UnloadSegmentComponents(Entity<CinematicComponent> ent, CinematicSegment segment)
    {
        if (segment.AddComp is { } components)
            EntityManager.RemoveComponents(ent, components);
    }

    private void LoadSegmentSound(Entity<CinematicComponent> ent, CinematicSegment segment)
    {
        if (segment.Sound == null || !IsWatching(ent))
            return;

        if (_audio.PlayGlobal(segment.Sound, Filter.Local(), false)?.Entity is not { } stream)
            return;

        ent.Comp.SegmentSounds.Add(stream);
        EnsureComp<CinematicSceneSoundComponent>(stream);
    }

    private void RaiseSegmentEvents(Entity<CinematicComponent> ent, CinematicSegment segment)
    {
        if (!_net.IsServer && !IsWatching(ent))
            return;

        foreach (var ev in segment.Events)
            RaiseLocalEvent(ent, ev, true);
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
