using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public static class HullStyler
{
    public static void Apply(Control root)
    {
        foreach (var child in root.Children)
        {
            Style(child);
            Apply(child);
        }
    }

    private static void Style(Control control)
    {
        switch (control)
        {
            case ColorableSlider slider:
                slider.AddStyleClass(HullStyleClasses.ColorSlider);
                break;
            case Slider slider:
                slider.AddStyleClass(HullStyleClasses.Slider);
                break;
            case LineEdit edit:
                edit.AddStyleClass(HullStyleClasses.Field);
                break;
            case OptionButton option:
                StyleOptionButton(option);
                break;
            case ContainerButton button:
                button.RemoveStyleClass(SpinBox.LeftButtonStyle);
                button.RemoveStyleClass(SpinBox.RightButtonStyle);
                button.AddStyleClass(HullStyleClasses.Plate);
                break;
            case Label label when !InsideButton(label):
                label.AddStyleClass(HullStyleClasses.FieldLabel);
                break;
        }
    }

    private static void StyleOptionButton(OptionButton option)
    {
        option.AddStyleClass(HullStyleClasses.Plate);
        option.OptionStyleClasses.Add(HullStyleClasses.MenuItem);
        RemoveClassFromLabels(option, OptionButton.StyleClassOptionButton);

        StyleOptions(option.OptionsScroll);

        if (option.OptionsScroll.Parent is not { } popup)
            return;

        foreach (var child in popup.Children)
        {
            if (child is not PanelContainer panel || !panel.HasStyleClass(OptionButton.StyleClassOptionsBackground))
                continue;

            panel.RemoveStyleClass(OptionButton.StyleClassOptionsBackground);
            panel.AddStyleClass(HullStyleClasses.MenuPopup);
        }
    }

    private static void StyleOptions(Control root)
    {
        foreach (var child in root.Children)
        {
            switch (child)
            {
                case Button button:
                    button.AddStyleClass(HullStyleClasses.MenuItem);
                    break;
                case LineEdit edit:
                    edit.AddStyleClass(HullStyleClasses.Field);
                    break;
            }

            StyleOptions(child);
        }
    }

    private static void RemoveClassFromLabels(Control root, string styleClass)
    {
        foreach (var child in root.Children)
        {
            if (child is Label label)
                label.RemoveStyleClass(styleClass);

            RemoveClassFromLabels(child, styleClass);
        }
    }

    private static bool InsideButton(Control control)
    {
        for (var parent = control.Parent; parent != null; parent = parent.Parent)
        {
            if (parent is BaseButton)
                return true;
        }

        return false;
    }
}
