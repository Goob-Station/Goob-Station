using Content.Client._Goobstation.UserInterface.Controls;
using Content.Client.UserInterface;
using Content.Client.UserInterface.Controls;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Silicons.IPC;

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Silicons.IPC;

public sealed class ScreenSaverUIController : UIController
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly MarkingManager _markingManager = default!;

    private PaginatedRadialMenu? _menu;
    private EntityUid? _lastUser;



    public void ToggleMenu(EntityUid user)
    {
        if (_menu == null)
        {
            SpriteSpecifier? headSprite = null;
            Color? headColor = null;

            if (_entityManager.TryGetComponent<HumanoidAppearanceComponent>(user, out var humanoid))
            {
                foreach (var marking in humanoid.MarkingSet.GetForwardEnumerator())
                {
                    if (_prototypeManager.TryIndex<MarkingPrototype>(marking.MarkingId, out var proto))
                    {
                        if (proto.MarkingCategory == MarkingCategories.Head)
                        {
                            if (proto.Sprites.Count == 0)
                                continue;

                            headSprite = proto.Sprites[0];
                            if (marking.MarkingColors.Count > 0)
                                headColor = marking.MarkingColors[0];
                            break;
                        }
                    }
                }
            }

            if (headSprite == null)
            {
                if (_prototypeManager.TryIndex("MobIPCHeadDefault", out MarkingPrototype? defaultHead) && defaultHead.Sprites.Count > 0)
                {
                    headSprite = defaultHead.Sprites[0];
                }
            }

            var screens = _markingManager.Markings;
            var options = new List<RadialMenuOptionBase>();

            foreach (var markingPrototype in screens.Values)
            {
                if (markingPrototype.MarkingCategory != MarkingCategories.Face)
                    continue;

                if (markingPrototype.SpeciesRestrictions != null &&
                    !markingPrototype.SpeciesRestrictions.Contains("IPC"))
                    continue;

                if (markingPrototype.Sprites.Count == 0)
                    continue;

                var screenSprite = markingPrototype.Sprites[0];
                var actionOption = new RadialMenuScreenSaverOption(
                    () => { SelectScreen(markingPrototype); },
                    headSprite,
                    screenSprite,
                    headColor)
                {
                    ToolTip = Loc.GetString($"marking-{markingPrototype.ID}")
                };

                options.Add(actionOption);
            }

            if (options.Count == 0)
            {
                return;
            }

            _menu = new PaginatedRadialMenu();
            _menu.SetButtons(options);
            _menu.OnClose += OnWindowClosed;
            _lastUser = user;
            _menu.OpenCentered();
        }
        else
        {
            _menu.OnClose -= OnWindowClosed;
            CloseMenu();
        }
    }

    private void SelectScreen(MarkingPrototype proto)
    {
        _entityManager.RaisePredictiveEvent(new SelectScreenSaverMessage(proto.ID));
    }

    private void OnWindowClosed()
    {
        _menu = null;
        _lastUser = null;
    }

    private void CloseMenu()
    {
        if (_menu == null) return;

        _menu.Close();
        _menu = null;
        _lastUser = null;
    }
}
