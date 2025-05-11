// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

namespace Content.Goobstation.Shared.MisandryBox.Helpers;

public static class ColorExt
{
    // Don't divide by zero by accident
    private const double Epsilon = 1e-10;

    public static HSV ToHSV(this Color color)
    {

        var red = color.R / 255.0;
        var green = color.G / 255.0;
        var blue = color.B / 255.0;

        var max = Math.Max(red, Math.Max(green, blue));
        var min = Math.Min(red, Math.Min(green, blue));
        var delta = max - min;

        var v = max;
        var s = Math.Abs(max) < Epsilon ? 0 : delta / max;
        var h = 0.0;

        if (!(delta > Epsilon))
            return new HSV(h, s, v);

        if (Math.Abs(max - red) < Epsilon)
        {
            // yellow, magenta
            h = (green - blue) / delta + (green < blue ? 6 : 0);
        }
        else if (Math.Abs(max - green) < Epsilon)
        {
            // cyan, yellow
            h = (blue - red) / delta + 2;
        }
        else // max is blue
        {
            // magenta and cyan
            h = (red - green) / delta + 4;
        }

        h *= 60;

        return new HSV(h, s, v);
    }
}

public record HSV(double H, double S, double V);
