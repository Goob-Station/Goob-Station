# SPDX-FileCopyrightText: 2022 Eoin Mcloughlin <helloworld@eoinrul.es>
# SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
# SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
# SPDX-FileCopyrightText: 2023 Ilya246 <57039557+Ilya246@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Southbridge <7013162+southbridge-fur@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# UI

## Fenêtre

air-alarm-ui-title = Alarme Atmosphérique

air-alarm-ui-access-denied = Accès insuffisant !

air-alarm-ui-window-pressure-label = Pression
air-alarm-ui-window-temperature-label = Température
air-alarm-ui-window-alarm-state-label = Statut

air-alarm-ui-window-address-label = Adresse
air-alarm-ui-window-device-count-label = Total des appareils
air-alarm-ui-window-resync-devices-label = Resynchroniser

air-alarm-ui-window-mode-label = Mode
air-alarm-ui-window-mode-select-locked-label = [bold][color=red] Échec du sélecteur de mode ! [/color][/bold]
air-alarm-ui-window-auto-mode-label = Mode automatique

-air-alarm-state-name = { $state ->
    [normal] Normal
    [warning] Avertissement
    [danger] Danger
    [emagged] Piraté
   *[invalid] Invalide
}

air-alarm-ui-window-listing-title = {$address} : {-air-alarm-state-name(state:$state)}
air-alarm-ui-window-pressure = {$pressure} kPa
air-alarm-ui-window-pressure-indicator = Pression : [color={$color}]{$pressure} kPa[/color]
air-alarm-ui-window-temperature = {$tempC} C ({$temperature} K)
air-alarm-ui-window-temperature-indicator = Température : [color={$color}]{$tempC} C ({$temperature} K)[/color]
air-alarm-ui-window-alarm-state = [color={$color}]{-air-alarm-state-name(state:$state)}[/color]
air-alarm-ui-window-alarm-state-indicator = Statut : [color={$color}]{-air-alarm-state-name(state:$state)}[/color]

air-alarm-ui-window-tab-vents = Ventilateurs
air-alarm-ui-window-tab-scrubbers = Épurateurs
air-alarm-ui-window-tab-sensors = Capteurs

air-alarm-ui-gases = {$gas} : {$amount} mol ({$percentage}%)
air-alarm-ui-gases-indicator = {$gas} : [color={$color}]{$amount} mol ({$percentage}%)[/color]

air-alarm-ui-mode-filtering = Filtration
air-alarm-ui-mode-wide-filtering = Filtration (large)
air-alarm-ui-mode-fill = Remplissage
air-alarm-ui-mode-panic = Panique
air-alarm-ui-mode-none = Aucun


air-alarm-ui-pump-direction-siphoning = Aspiration
air-alarm-ui-pump-direction-scrubbing = Épuration
air-alarm-ui-pump-direction-releasing = Libération

air-alarm-ui-pressure-bound-nobound = Sans limite
air-alarm-ui-pressure-bound-internalbound = Limite interne
air-alarm-ui-pressure-bound-externalbound = Limite externe
air-alarm-ui-pressure-bound-both = Les deux

air-alarm-ui-widget-gas-filters = Filtres à gaz

## Composants

### Général

air-alarm-ui-widget-enable = Activé
air-alarm-ui-widget-copy = Copier les paramètres vers les appareils similaires
air-alarm-ui-widget-copy-tooltip = Copie les paramètres de cet appareil vers tous les appareils de cet onglet d'alarme atmosphérique.
air-alarm-ui-widget-ignore = Ignorer
air-alarm-ui-atmos-net-device-label = Adresse : {$address}

### Pompes de ventilation

air-alarm-ui-vent-pump-label = Direction de ventilation
air-alarm-ui-vent-pressure-label = Limite de pression
air-alarm-ui-vent-external-bound-label = Limite externe
air-alarm-ui-vent-internal-bound-label = Limite interne

### Épurateurs

air-alarm-ui-scrubber-pump-direction-label = Direction
air-alarm-ui-scrubber-volume-rate-label = Débit (L)
air-alarm-ui-scrubber-wide-net-label = Réseau large
air-alarm-ui-scrubber-select-all-gases-label = Tout sélectionner
air-alarm-ui-scrubber-deselect-all-gases-label = Tout désélectionner

### Seuils

air-alarm-ui-sensor-gases = Gaz
air-alarm-ui-sensor-thresholds = Seuils
air-alarm-ui-thresholds-pressure-title = Seuils (kPa)
air-alarm-ui-thresholds-temperature-title = Seuils (K)
air-alarm-ui-thresholds-gas-title = Seuils (%)
air-alarm-ui-thresholds-upper-bound = Danger au-dessus
air-alarm-ui-thresholds-lower-bound = Danger en-dessous
air-alarm-ui-thresholds-upper-warning-bound = Avertissement au-dessus
air-alarm-ui-thresholds-lower-warning-bound = Avertissement en-dessous
air-alarm-ui-thresholds-copy = Copier les seuils vers tous les appareils
air-alarm-ui-thresholds-copy-tooltip = Copie les seuils du capteur de cet appareil vers tous les appareils de cet onglet d'alarme atmosphérique.
