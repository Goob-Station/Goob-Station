using System.Linq;

namespace Content.Goobstation.Shared.TrackedComponents;

public sealed partial class TrackedComponentsSystem : EntitySystem
{
    public T EnsureTrackedComp<T>(EntityUid uid, string owner, T? preconfigured = null) where T : Component, new()
    {
        var tracking = EnsureComp<TrackedComponentsComponent>(uid);

        if (!tracking.ComponentsByOwner.TryGetValue(owner, out var components))
        {
            components = [];
            tracking.ComponentsByOwner[owner] = components;
        }

        var componentName = typeof(T).FullName;
        if (componentName == null)
            return EnsureComp<T>(uid);

        components.Add(componentName);

        if (TryComp<T>(uid, out var existing))
            return existing;

        tracking.TrackerCreatedComponents.Add(componentName);

        if (preconfigured != null)
        {
            AddComp(uid, preconfigured);
            return preconfigured;
        }

        return EnsureComp<T>(uid);
    }

    public void RemoveTrackedComps(EntityUid uid, string owner)
    {
        if (!TryComp<TrackedComponentsComponent>(uid, out var tracking)
            || !tracking.ComponentsByOwner.Remove(owner, out var components))
            return;

        foreach (var componentName in components)
        {
            var stillClaimed = tracking.ComponentsByOwner.Values
                .Any(otherComponents => otherComponents.Contains(componentName));

            if (stillClaimed)
                continue;

            if (!tracking.TrackerCreatedComponents.Remove(componentName))
                continue;

            // No more claims, we can get rid of it.
            foreach (var comp in EntityManager.GetComponents(uid))
            {
                if (comp.GetType().FullName == componentName)
                {
                    RemCompDeferred(uid, comp.GetType());
                    break;
                }
            }

        }

        // No more to track, remove component.
        if (tracking.ComponentsByOwner.Count == 0)
            RemCompDeferred<TrackedComponentsComponent>(uid);
    }
}
