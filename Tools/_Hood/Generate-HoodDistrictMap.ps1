# SPDX-License-Identifier: AGPL-3.0-or-later

param(
    [string] $OutputPath = "Resources/Maps/_Hood/hood_district.yml"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$invariant = [System.Globalization.CultureInfo]::InvariantCulture
$groups = [ordered]@{}
$nextUid = 3

function Add-MapEntity {
    param(
        [Parameter(Mandatory)] [string] $Prototype,
        [Parameter(Mandatory)] [double] $X,
        [Parameter(Mandatory)] [double] $Y,
        [string] $Rotation,
        [string] $Text,
        [string] $Color = "#FFFFFFFF",
        [int] $FontSize = 12
    )

    if (-not $groups.Contains($Prototype)) {
        $groups[$Prototype] = [System.Collections.Generic.List[object]]::new()
    }

    $entry = [pscustomobject]@{
        Uid = $script:nextUid
        X = $X
        Y = $Y
        Rotation = $Rotation
        Text = $Text
        Color = $Color
        FontSize = $FontSize
    }

    $script:nextUid++
    $groups[$Prototype].Add($entry)
}

function Add-RectangleWalls {
    param(
        [Parameter(Mandatory)] [string] $Prototype,
        [Parameter(Mandatory)] [int] $X1,
        [Parameter(Mandatory)] [int] $Y1,
        [Parameter(Mandatory)] [int] $X2,
        [Parameter(Mandatory)] [int] $Y2,
        [string[]] $Openings = @()
    )

    $openingSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($opening in $Openings) {
        [void] $openingSet.Add($opening)
    }

    $points = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($x in $X1..$X2) {
        [void] $points.Add("$x,$Y1")
        [void] $points.Add("$x,$Y2")
    }

    foreach ($y in $Y1..$Y2) {
        [void] $points.Add("$X1,$y")
        [void] $points.Add("$X2,$y")
    }

    foreach ($point in ($points | Sort-Object)) {
        if ($openingSet.Contains($point)) {
            continue
        }

        $parts = $point.Split(',')
        Add-MapEntity -Prototype $Prototype -X ([int] $parts[0] + 0.5) -Y ([int] $parts[1] + 0.5)
    }
}

function Get-TileYamlId {
    param([int] $X, [int] $Y)

    # Grass is the district's default open ground.
    $tile = 1

    # Building and plaza surfaces.
    if ($X -ge 2 -and $X -le 25 -and $Y -ge 31 -and $Y -le 45) { $tile = 4 }
    if ($X -ge 39 -and $X -le 62 -and $Y -ge 31 -and $Y -le 45) { $tile = 5 }
    if ($X -ge 2 -and $X -le 12 -and $Y -ge 10 -and $Y -le 18) { $tile = 6 }
    if ($X -ge 14 -and $X -le 25 -and $Y -ge 10 -and $Y -le 18) { $tile = 7 }
    if ($X -ge 2 -and $X -le 12 -and $Y -ge 2 -and $Y -le 8) { $tile = 8 }
    if ($X -ge 14 -and $X -le 25 -and $Y -ge 2 -and $Y -le 8) { $tile = 9 }
    if ($X -ge 39 -and $X -le 49 -and $Y -ge 10 -and $Y -le 18) { $tile = 3 }
    if ($X -ge 51 -and $X -le 62 -and $Y -ge 2 -and $Y -le 18) { $tile = 7 }
    if ($X -ge 39 -and $X -le 49 -and $Y -ge 2 -and $Y -le 8) { $tile = 3 }

    # Walkable alleys and short paths between storefronts, homes, and sidewalks.
    if (($X -ge 1 -and $X -le 26 -and $Y -eq 9) -or
        ($X -eq 13 -and $Y -ge 1 -and $Y -le 18) -or
        ($X -ge 26 -and $X -le 28 -and $Y -ge 1 -and $Y -le 18) -or
        ($X -ge 37 -and $X -le 38 -and $Y -ge 1 -and $Y -le 18) -or
        ($X -eq 50 -and $Y -ge 1 -and $Y -le 18) -or
        ($X -ge 12 -and $X -le 13 -and $Y -ge 29 -and $Y -le 31) -or
        ($X -ge 50 -and $X -le 51 -and $Y -ge 29 -and $Y -le 31)) {
        $tile = 3
    }

    # Concrete sidewalks border both streets.
    if (($X -ge 27 -and $X -le 28) -or
        ($X -ge 35 -and $X -le 36) -or
        ($Y -ge 19 -and $Y -le 20) -or
        ($Y -ge 27 -and $Y -le 28)) {
        $tile = 3
    }

    # Two crossing asphalt streets form the primary circulation spine.
    if (($X -ge 29 -and $X -le 34) -or ($Y -ge 21 -and $Y -le 26)) {
        $tile = 2
    }

    # A compact street-facing lot provides an explicit parking/test area without
    # cutting either faction building off from its two independent approaches.
    if ($X -ge 38 -and $X -le 50 -and $Y -ge 27 -and $Y -le 30) {
        $tile = 2
    }

    return $tile
}

function New-TileChunk {
    param([int] $ChunkX, [int] $ChunkY)

    $bytes = [byte[]]::new(16 * 16 * 7)
    $offset = 0

    foreach ($localY in 0..15) {
        foreach ($localX in 0..15) {
            $x = $ChunkX * 16 + $localX
            $y = $ChunkY * 16 + $localY
            $tileId = Get-TileYamlId -X $x -Y $y
            $idBytes = [System.BitConverter]::GetBytes([int] $tileId)
            [System.Array]::Copy($idBytes, 0, $bytes, $offset, 4)
            $offset += 4
            $bytes[$offset++] = 0 # flags
            $bytes[$offset++] = 0 # variant
            $bytes[$offset++] = 0 # rotation and mirroring
        }
    }

    return [Convert]::ToBase64String($bytes)
}

# Impassable district boundary. Horizontal fence sections are rotated.
foreach ($x in 1..62) {
    if ($x -ne 31) {
        Add-MapEntity -Prototype FenceMetalStraight -X ($x + 0.5) -Y 0.5 -Rotation "1.5707963267948966 rad"
    }
    Add-MapEntity -Prototype FenceMetalStraight -X ($x + 0.5) -Y 47.5 -Rotation "1.5707963267948966 rad"
}
foreach ($y in 1..46) {
    Add-MapEntity -Prototype FenceMetalStraight -X 0.5 -Y ($y + 0.5)
    Add-MapEntity -Prototype FenceMetalStraight -X 63.5 -Y ($y + 0.5)
}
foreach ($corner in @(@(0.5, 0.5), @(63.5, 0.5), @(0.5, 47.5), @(63.5, 47.5))) {
    Add-MapEntity -Prototype WallBrick -X $corner[0] -Y $corner[1]
}
# A manually operated chain-link gate opens directly onto the north/south road.
Add-MapEntity -Prototype FenceMetalGate -X 31.5 -Y 0.5

# Faction buildings have two independent two-tile exits each.
Add-RectangleWalls -Prototype WallBrick -X1 2 -Y1 31 -X2 25 -Y2 45 -Openings @("12,31", "13,31", "25,38", "25,39")
Add-RectangleWalls -Prototype WallBrick -X1 39 -Y1 31 -X2 62 -Y2 45 -Openings @("50,31", "51,31", "39,38", "39,39")

# Commercial row, underground workshop, corner market, and residential block.
Add-RectangleWalls -Prototype WallBrick -X1 2 -Y1 10 -X2 12 -Y2 18 -Openings @("6,18", "7,18")
Add-RectangleWalls -Prototype WallBrick -X1 14 -Y1 10 -X2 25 -Y2 18 -Openings @("19,18", "20,18")
Add-RectangleWalls -Prototype WallBrick -X1 2 -Y1 2 -X2 12 -Y2 8 -Openings @("6,8", "7,8")
Add-RectangleWalls -Prototype WallBrick -X1 14 -Y1 2 -X2 25 -Y2 8 -Openings @("19,8", "20,8", "25,5", "25,6")
Add-RectangleWalls -Prototype WallBrick -X1 39 -Y1 10 -X2 49 -Y2 18 -Openings @("43,18", "44,18")
Add-RectangleWalls -Prototype WallBrick -X1 51 -Y1 2 -X2 62 -Y2 18 -Openings @("56,18", "57,18", "51,6", "51,7")

# Simple residential room divisions retain open, access-free passages.
foreach ($x in 52..61) {
    if ($x -notin 56, 57) { Add-MapEntity -Prototype WallWood -X ($x + 0.5) -Y 9.5 }
}
foreach ($y in 3..8) {
    if ($y -ne 5) { Add-MapEntity -Prototype WallWood -X 56.5 -Y ($y + 0.5) }
}
foreach ($y in 10..17) {
    if ($y -ne 14) { Add-MapEntity -Prototype WallWood -X 56.5 -Y ($y + 0.5) }
}

# Crisps interior and meeting space.
foreach ($position in @(@(6.5, 37.5), @(18.5, 41.5))) { Add-MapEntity -Prototype TableFancyBlue -X $position[0] -Y $position[1] }
foreach ($position in @(@(7.5, 36.5), @(19.5, 40.5))) { Add-MapEntity -Prototype BenchBlueComfy -X $position[0] -Y $position[1] }
foreach ($position in @(@(4.5, 43.5), @(23.5, 43.5))) { Add-MapEntity -Prototype Rack -X $position[0] -Y $position[1] }
foreach ($position in @(@(10.5, 37.5), @(21.5, 41.5))) { Add-MapEntity -Prototype ChairWood -X $position[0] -Y $position[1] }
Add-MapEntity -Prototype PottedPlant1 -X 3.5 -Y 32.5

# Buds interior mirrors the Crisps opportunities and furnishing density.
foreach ($position in @(@(45.5, 41.5), @(57.5, 37.5))) { Add-MapEntity -Prototype TableFancyGreen -X $position[0] -Y $position[1] }
foreach ($position in @(@(44.5, 40.5), @(56.5, 36.5))) { Add-MapEntity -Prototype BenchColorfulComfy -X $position[0] -Y $position[1] }
foreach ($position in @(@(40.5, 43.5), @(60.5, 43.5))) { Add-MapEntity -Prototype Rack -X $position[0] -Y $position[1] }
foreach ($position in @(@(42.5, 41.5), @(53.5, 37.5))) { Add-MapEntity -Prototype ChairWood -X $position[0] -Y $position[1] }
Add-MapEntity -Prototype PottedPlant4 -X 61.5 -Y 32.5

# Electronics store: counters, stock racks, display phones, and a physical register.
foreach ($x in 4..7) { Add-MapEntity -Prototype TableCounterWood -X ($x + 0.5) -Y 15.5 }
foreach ($position in @(@(3.5, 11.5), @(10.5, 11.5))) { Add-MapEntity -Prototype Rack -X $position[0] -Y $position[1] }
Add-MapEntity -Prototype HoodCashRegister -X 9.5 -Y 15.5
Add-MapEntity -Prototype ComputerTelevision -X 10.5 -Y 13.5
Add-MapEntity -Prototype HoodPhoneStreetline -X 4.5 -Y 15.5
Add-MapEntity -Prototype HoodPhoneSunset -X 5.5 -Y 15.5
Add-MapEntity -Prototype HoodPhonePacific -X 6.5 -Y 15.5
Add-MapEntity -Prototype HoodPhoneStreetlineBox -X 3.5 -Y 11.5
Add-MapEntity -Prototype HoodPhoneSunsetBox -X 10.5 -Y 11.5
foreach ($x in 7..9) { Add-MapEntity -Prototype HoodSimCard -X ($x + 0.5) -Y 15.5 }

# Smoke shop.
foreach ($x in 16..19) { Add-MapEntity -Prototype TableCounterWood -X ($x + 0.5) -Y 15.5 }
foreach ($position in @(@(15.5, 11.5), @(23.5, 11.5), @(23.5, 14.5))) { Add-MapEntity -Prototype Rack -X $position[0] -Y $position[1] }
Add-MapEntity -Prototype HoodCashRegister -X 21.5 -Y 15.5
foreach ($stock in @(
    @("CigPackGreen", 15.5, 11.5), @("CigPackRed", 23.5, 11.5),
    @("CigarCase", 23.5, 14.5), @("PackPaperRollingFilters", 16.5, 15.5),
    @("Lighter", 17.5, 15.5), @("Matchbox", 18.5, 15.5))) {
    Add-MapEntity -Prototype $stock[0] -X $stock[1] -Y $stock[2]
}

# Gun store displays the fictional Hood line and matching shared ammunition.
foreach ($x in 4..7) { Add-MapEntity -Prototype TableCounterWood -X ($x + 0.5) -Y 6.5 }
Add-MapEntity -Prototype HoodCashRegister -X 9.5 -Y 6.5
Add-MapEntity -Prototype GunSafe -X 3.5 -Y 3.5
Add-MapEntity -Prototype GunSafe -X 10.5 -Y 3.5
Add-MapEntity -Prototype Rack -X 10.5 -Y 5.5
Add-MapEntity -Prototype HoodWeaponPistolGlorpG1 -X 4.5 -Y 6.5
Add-MapEntity -Prototype HoodWeaponPistolGlorpC2 -X 5.5 -Y 6.5
Add-MapEntity -Prototype HoodWeaponPistolGlorpS3 -X 6.5 -Y 6.5
Add-MapEntity -Prototype HoodWeaponPistolGlorpL4 -X 7.5 -Y 6.5
Add-MapEntity -Prototype HoodGunSwitch -X 10.5 -Y 5.5
Add-MapEntity -Prototype HoodWeaponCarbineRookC9 -X 3.5 -Y 3.5
Add-MapEntity -Prototype HoodWeaponShotgunMesaP12 -X 10.5 -Y 3.5
Add-MapEntity -Prototype MagazinePistol -X 8.5 -Y 6.5
Add-MapEntity -Prototype MagazinePistolSubMachineGun -X 9.5 -Y 6.5
Add-MapEntity -Prototype MagazineBoxPistol -X 10.5 -Y 5.5
Add-MapEntity -Prototype BoxLethalshot -X 10.5 -Y 3.5

# Underground seller/workshop blockout.
foreach ($x in 16..20) { Add-MapEntity -Prototype TableCounterMetal -X ($x + 0.5) -Y 6.5 }
Add-MapEntity -Prototype HoodCashRegister -X 21.5 -Y 6.5
Add-MapEntity -Prototype CrateSecure -X 15.5 -Y 3.5
Add-MapEntity -Prototype Rack -X 23.5 -Y 3.5
Add-MapEntity -Prototype HoodUndergroundPrinter -X 23.5 -Y 6.5
Add-MapEntity -Prototype HoodGunSwitch -X 20.5 -Y 6.5
Add-MapEntity -Prototype HoodWeaponRifleArroyoR12 -X 15.5 -Y 3.5
Add-MapEntity -Prototype MagazineLightRifle -X 19.5 -Y 6.5
Add-MapEntity -Prototype MagazineBoxLightRifle -X 23.5 -Y 3.5

# Neutral corner market.
foreach ($x in 41..44) { Add-MapEntity -Prototype TableCounterWood -X ($x + 0.5) -Y 15.5 }
Add-MapEntity -Prototype HoodCashRegister -X 46.5 -Y 15.5
foreach ($position in @(@(40.5, 11.5), @(47.5, 11.5), @(47.5, 14.5))) { Add-MapEntity -Prototype Rack -X $position[0] -Y $position[1] }
Add-MapEntity -Prototype HoodClothingUniformBaggyCream -X 41.5 -Y 15.5
Add-MapEntity -Prototype HoodClothingUniformCargoGraphic -X 42.5 -Y 15.5
Add-MapEntity -Prototype HoodClothingOuterBlackZipHoodie -X 43.5 -Y 15.5
Add-MapEntity -Prototype HoodClothingOuterNavyVarsity -X 44.5 -Y 15.5
Add-MapEntity -Prototype HoodClothingShoesWhiteLowtops -X 47.5 -Y 14.5
Add-MapEntity -Prototype HoodClothingHeadCharcoalFittedCap -X 40.5 -Y 11.5
Add-MapEntity -Prototype HoodClothingUniformTankWorkpants -X 40.5 -Y 11.5
Add-MapEntity -Prototype HoodClothingUniformForestPolo -X 47.5 -Y 11.5
Add-MapEntity -Prototype HoodClothingOuterCharcoalPuffer -X 47.5 -Y 14.5
Add-MapEntity -Prototype HoodClothingOuterBrownWorkJacket -X 41.5 -Y 15.5
Add-MapEntity -Prototype HoodClothingHeadBlackKnitBeanie -X 42.5 -Y 15.5
Add-MapEntity -Prototype HoodClothingEyesSmokeRectangular -X 43.5 -Y 15.5
foreach ($stock in @(
    @("DrinkWaterBottleFull", 40.5, 11.5), @("DrinkWaterBottleFull", 47.5, 11.5),
    @("FoodSnackChips", 47.5, 14.5), @("FoodSnackChips", 44.5, 15.5))) {
    Add-MapEntity -Prototype $stock[0] -X $stock[1] -Y $stock[2]
}

# Small physical-cash floats make the economy immediately testable without an account backend.
Add-MapEntity -Prototype HoodCash1 -X 45.5 -Y 15.5
Add-MapEntity -Prototype HoodCash5 -X 46.5 -Y 14.5
Add-MapEntity -Prototype HoodCash20 -X 47.5 -Y 15.5

# Residential rooms.
foreach ($position in @(@(53.5, 4.5), @(59.5, 4.5), @(53.5, 12.5), @(59.5, 12.5))) { Add-MapEntity -Prototype Bed -X $position[0] -Y $position[1] }
foreach ($position in @(@(54.5, 7.5), @(60.5, 7.5), @(54.5, 15.5), @(60.5, 15.5))) { Add-MapEntity -Prototype Dresser -X $position[0] -Y $position[1] }
foreach ($position in @(@(53.5, 7.5), @(59.5, 7.5), @(53.5, 15.5), @(59.5, 15.5))) { Add-MapEntity -Prototype ChairWood -X $position[0] -Y $position[1] }

# Community plaza and street lighting.
foreach ($position in @(@(41.5, 3.5), @(46.5, 3.5))) { Add-MapEntity -Prototype WoodenBench -X $position[0] -Y $position[1] }
Add-MapEntity -Prototype FloraTree -X 48.5 -Y 7.5
foreach ($position in @(@(4.5, 20.5), @(24.5, 20.5), @(39.5, 20.5), @(59.5, 20.5))) { Add-MapEntity -Prototype WoodenBench -X $position[0] -Y $position[1] }
foreach ($position in @(@(2.5, 20.5), @(26.5, 20.5), @(37.5, 20.5), @(61.5, 20.5), @(38.5, 29.5), @(50.5, 29.5))) { Add-MapEntity -Prototype FloraTree -X $position[0] -Y $position[1] }
foreach ($position in @(@(13.5, 3.5), @(26.5, 17.5), @(37.5, 17.5), @(50.5, 18.5))) { Add-MapEntity -Prototype CrateTrashCart -X $position[0] -Y $position[1] }
foreach ($position in @(
    @(28.5, 18.5), @(35.5, 18.5), @(28.5, 29.5), @(35.5, 29.5),
    @(12.5, 20.5), @(20.5, 20.5), @(43.5, 20.5), @(52.5, 20.5),
    @(40.5, 8.5), @(48.5, 8.5))) {
    Add-MapEntity -Prototype LightPostSmall -X $position[0] -Y $position[1]
}

# Exact Hood job spawn markers. Additional instances support multi-slot jobs.
Add-MapEntity -Prototype SpawnPointCrispsLeader -X 6.5 -Y 42.5
Add-MapEntity -Prototype SpawnPointCrispsMedia -X 10.5 -Y 40.5
foreach ($position in @(@(6.5, 34.5), @(10.5, 34.5), @(14.5, 34.5), @(18.5, 34.5))) { Add-MapEntity -Prototype SpawnPointCrispsMember -X $position[0] -Y $position[1] }
foreach ($position in @(@(18.5, 37.5), @(22.5, 37.5))) { Add-MapEntity -Prototype SpawnPointCrispsDealer -X $position[0] -Y $position[1] }

Add-MapEntity -Prototype SpawnPointBudsLeader -X 58.5 -Y 42.5
Add-MapEntity -Prototype SpawnPointBudsMedia -X 54.5 -Y 40.5
foreach ($position in @(@(44.5, 34.5), @(48.5, 34.5), @(52.5, 34.5), @(56.5, 34.5))) { Add-MapEntity -Prototype SpawnPointBudsMember -X $position[0] -Y $position[1] }
foreach ($position in @(@(42.5, 37.5), @(46.5, 37.5))) { Add-MapEntity -Prototype SpawnPointBudsDealer -X $position[0] -Y $position[1] }

foreach ($position in @(@(4.5, 13.5), @(6.5, 13.5), @(9.5, 13.5))) { Add-MapEntity -Prototype SpawnPointElectronicsStoreEmployee -X $position[0] -Y $position[1] }
foreach ($position in @(@(16.5, 13.5), @(19.5, 13.5), @(22.5, 13.5))) { Add-MapEntity -Prototype SpawnPointSmokeShopEmployee -X $position[0] -Y $position[1] }
foreach ($position in @(@(4.5, 5.5), @(6.5, 5.5), @(9.5, 5.5))) { Add-MapEntity -Prototype SpawnPointGunStoreEmployee -X $position[0] -Y $position[1] }
foreach ($position in @(@(17.5, 5.5), @(22.5, 5.5))) { Add-MapEntity -Prototype SpawnPointUndergroundSeller -X $position[0] -Y $position[1] }
foreach ($position in @(@(42.5, 5.5), @(45.5, 5.5), @(48.5, 5.5))) { Add-MapEntity -Prototype SpawnPointLatejoin -X $position[0] -Y $position[1] }

# World-space blockout labels make every gameplay zone immediately legible.
Add-MapEntity -Prototype MapText -X 13.5 -Y 44.0 -Text "CRISPS TERRITORY" -Color "#4775D1FF" -FontSize 18
Add-MapEntity -Prototype MapText -X 50.5 -Y 44.0 -Text "BUDS TERRITORY" -Color "#52A365FF" -FontSize 18
Add-MapEntity -Prototype MapText -X 7.0 -Y 17.0 -Text "ELECTRONICS" -Color "#7EB6FFFF" -FontSize 14
Add-MapEntity -Prototype MapText -X 19.5 -Y 17.0 -Text "SMOKE SHOP" -Color "#C49A6CFF" -FontSize 14
Add-MapEntity -Prototype MapText -X 7.0 -Y 7.0 -Text "GUN STORE" -Color "#D47D7DFF" -FontSize 14
Add-MapEntity -Prototype MapText -X 19.5 -Y 7.0 -Text "UNDERGROUND MARKET" -Color "#B592D0FF" -FontSize 13
Add-MapEntity -Prototype MapText -X 44.0 -Y 17.0 -Text "CORNER MARKET" -Color "#F0C66AFF" -FontSize 14
Add-MapEntity -Prototype MapText -X 56.5 -Y 17.0 -Text "RESIDENTIAL" -Color "#E7D4B5FF" -FontSize 14
Add-MapEntity -Prototype MapText -X 44.0 -Y 7.0 -Text "COMMUNITY PLAZA" -Color "#E6E6E6FF" -FontSize 14
Add-MapEntity -Prototype MapText -X 44.0 -Y 29.0 -Text "PARKING LOT" -Color "#F2E8B8FF" -FontSize 13
Add-MapEntity -Prototype MapText -X 13.5 -Y 20.0 -Text "COMMERCIAL ROW" -Color "#FFFFFFFF" -FontSize 13
Add-MapEntity -Prototype MapText -X 31.5 -Y 24.0 -Text "HOOD DISTRICT - MAIN STREET" -Color "#FFFFFFFF" -FontSize 14

$chunks = [System.Collections.Generic.List[object]]::new()
foreach ($chunkY in 0..2) {
    foreach ($chunkX in 0..3) {
        $chunks.Add([pscustomobject]@{
            X = $chunkX
            Y = $chunkY
            Tiles = New-TileChunk -ChunkX $chunkX -ChunkY $chunkY
        })
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
function Add-Line([string] $Line = "") { $lines.Add($Line) }

$entityCount = $nextUid - 1
Add-Line "# SPDX-License-Identifier: AGPL-3.0-or-later"
Add-Line
Add-Line "meta:"
Add-Line "  format: 7"
Add-Line "  category: Map"
Add-Line "  engineVersion: 268.0.0"
Add-Line "  forkId: ''"
Add-Line "  forkVersion: ''"
Add-Line "  time: 08/11/2026 20:00:00"
Add-Line "  entityCount: $entityCount"
Add-Line "maps:"
Add-Line "- 2"
Add-Line "grids:"
Add-Line "- 1"
Add-Line "orphans: []"
Add-Line "nullspace: []"
Add-Line "tilemap:"
Add-Line "  0: Space"
Add-Line "  1: FloorGrass"
Add-Line "  2: FloorAsphalt"
Add-Line "  3: FloorConcrete"
Add-Line "  4: FloorGrayConcrete"
Add-Line "  5: FloorOldConcrete"
Add-Line "  6: FloorConcreteSmooth"
Add-Line "  7: FloorWood"
Add-Line "  8: FloorGrayConcreteSmooth"
Add-Line "  9: FloorOldConcreteSmooth"
Add-Line "entities:"
Add-Line "- proto: ''"
Add-Line "  entities:"
Add-Line "  - uid: 1"
Add-Line "    components:"
Add-Line "    - type: MetaData"
Add-Line "      name: Hood District ground"
Add-Line "    - type: Transform"
Add-Line "      parent: 2"
Add-Line "    - type: MapGrid"
Add-Line "      chunks:"
foreach ($chunk in $chunks) {
    Add-Line "        $($chunk.X),$($chunk.Y):"
    Add-Line "          ind: $($chunk.X),$($chunk.Y)"
    Add-Line "          tiles: $($chunk.Tiles)"
    Add-Line "          version: 7"
}
Add-Line "    - type: Broadphase"
Add-Line "    - type: Physics"
Add-Line "      bodyStatus: InAir"
Add-Line "      angularDamping: 0.05"
Add-Line "      linearDamping: 0.05"
Add-Line "      fixedRotation: True"
Add-Line "      bodyType: Dynamic"
Add-Line "    - type: Fixtures"
Add-Line "      fixtures: {}"
Add-Line "    - type: BecomesStation"
Add-Line "      id: HoodDistrict"
Add-Line "    - type: OccluderTree"
Add-Line "    - type: GridPathfinding"
Add-Line "    - type: Gravity"
Add-Line "      inherent: True"
Add-Line "      enabled: True"
Add-Line "      gravityShakeSound: !type:SoundPathSpecifier"
Add-Line "        path: /Audio/Effects/alert.ogg"
Add-Line "    - type: DecalGrid"
Add-Line "      chunkCollection:"
Add-Line "        version: 2"
Add-Line "        nodes:"
Add-Line "        - node:"
Add-Line "            color: '#F2E8B8FF'"
Add-Line "            id: BoxGreyscale"
Add-Line "          decals:"
Add-Line "            1: 39,29"
Add-Line "            2: 41.5,29"
Add-Line "            3: 44,29"
Add-Line "            4: 46.5,29"
Add-Line "            5: 49,29"
Add-Line "    - type: SpreaderGrid"
Add-Line "  - uid: 2"
Add-Line "    components:"
Add-Line "    - type: MetaData"
Add-Line "      name: Hood District Blockout"
Add-Line "    - type: Transform"
Add-Line "    - type: Map"
Add-Line "      mapPaused: True"
Add-Line "    - type: GridTree"
Add-Line "    - type: Broadphase"
Add-Line "    - type: OccluderTree"
Add-Line "    - type: MapLight"
Add-Line "      ambientLightColor: '#D8C8A8FF'"
Add-Line "    - type: Parallax"
Add-Line "      parallax: Sky"
Add-Line "    - type: MapAtmosphere"
Add-Line "      space: False"
Add-Line "      mixture:"
Add-Line "        volume: 2500"
Add-Line "        immutable: True"
Add-Line "        temperature: 293.15"
Add-Line "        moles:"
Add-Line "        - 21.82478"
Add-Line "        - 82.10312"
foreach ($unused in 1..10) { Add-Line "        - 0" }

foreach ($prototype in $groups.Keys) {
    Add-Line "- proto: $prototype"
    Add-Line "  entities:"
    foreach ($entity in $groups[$prototype]) {
        Add-Line "  - uid: $($entity.Uid)"
        Add-Line "    components:"
        Add-Line "    - type: Transform"
        if ($entity.Rotation) { Add-Line "      rot: $($entity.Rotation)" }
        $x = $entity.X.ToString("0.###", $invariant)
        $y = $entity.Y.ToString("0.###", $invariant)
        Add-Line "      pos: $x,$y"
        Add-Line "      parent: 1"
        if ($prototype -eq "MapText") {
            $escapedText = $entity.Text.Replace("'", "''")
            Add-Line "    - type: MapText"
            Add-Line "      fontSize: $($entity.FontSize)"
            Add-Line "      color: '$($entity.Color)'"
            Add-Line "      text: '$escapedText'"
        }
    }
}
Add-Line "..."

$absoluteOutput = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath
} else {
    Join-Path (Get-Location) $OutputPath
}

$directory = Split-Path -Parent $absoluteOutput
[System.IO.Directory]::CreateDirectory($directory) | Out-Null
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($absoluteOutput, ($lines -join "`n") + "`n", $utf8NoBom)

Write-Output "Generated $absoluteOutput with $entityCount entities and $($chunks.Count) tile chunks."
