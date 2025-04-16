// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Maths;

namespace Content.Shared.Temperature
{
    public static class TemperatureHelpers
    {
        public static float CelsiusToKelvin(float celsius)
        {
            return celsius + PhysicalConstants.ZERO_CELCIUS;
        }

        public static float CelsiusToFahrenheit(float celsius)
        {
            return celsius * 9 / 5 + 32;
        }

        public static float KelvinToCelsius(float kelvin)
        {
            return kelvin - PhysicalConstants.ZERO_CELCIUS;
        }

        public static float KelvinToFahrenheit(float kelvin)
        {
            var celsius = KelvinToCelsius(kelvin);
            return CelsiusToFahrenheit(celsius);
        }

        public static float FahrenheitToCelsius(float fahrenheit)
        {
            return (fahrenheit - 32) * 5 / 9;
        }

        public static float FahrenheitToKelvin(float fahrenheit)
        {
            var celsius = FahrenheitToCelsius(fahrenheit);
            return CelsiusToKelvin(celsius);
        }
    }
}