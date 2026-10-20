Add-Type -AssemblyName System.Drawing

$repoRoot = "E:\goob\Goob-Station"
$bgPath = Join-Path $repoRoot "Resources\Textures\Interface\Actions\actions_borg.rsi\no-action.png"
$drillPath = Join-Path $repoRoot "Resources\Textures\_Lavaland\Objects\Tools\handdrill_super.rsi\handdrill.png"

$actionOutDir = Join-Path $repoRoot "Resources\Textures\_Goobstation\Actions\actions_borg.rsi"
$actionOutPath = Join-Path $actionOutDir "super-mining-module.png"

if (-not (Test-Path $actionOutDir)) { New-Item -ItemType Directory -Force -Path $actionOutDir | Out-Null }

$bg = [System.Drawing.Image]::FromFile($bgPath)
$drill = New-Object System.Drawing.Bitmap $drillPath

# 1. Measure non-transparent bounding box of the super drill
$minX = 32; $maxX = 0; $minY = 32; $maxY = 0
for ($y = 0; $y -lt $drill.Height; $y++) {
    for ($x = 0; $x -lt $drill.Width; $x++) {
        if ($drill.GetPixel($x, $y).A -gt 0) {
            if ($x -lt $minX) { $minX = $x }
            if ($x -gt $maxX) { $maxX = $x }
            if ($y -lt $minY) { $minY = $y }
            if ($y -gt $maxY) { $maxY = $y }
        }
    }
}

$drillW = $maxX - $minX + 1
$drillH = $maxY - $minY + 1

# 2. Create the composite bitmap
$bmp = New-Object System.Drawing.Bitmap 32, 32, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None

# Draw standard dark-blue scanline background
$g.DrawImage($bg, 0, 0, 32, 32)

# Fit cleanly inside 28px bounds so it doesn't touch the outer 1px action borders
$maxAllowed = 28.0
$scale = 1.0
if ($drillW -gt $maxAllowed -or $drillH -gt $maxAllowed) {
    $scale = [Math]::Min($maxAllowed / $drillW, $maxAllowed / $drillH)
}

$destW = [int][Math]::Round($drillW * $scale)
$destH = [int][Math]::Round($drillH * $scale)
$destX = [int][Math]::Round((32 - $destW) / 2.0)
$destY = [int][Math]::Round((32 - $destH) / 2.0)

$srcRect = New-Object System.Drawing.Rectangle $minX, $minY, $drillW, $drillH
$destRect = New-Object System.Drawing.Rectangle $destX, $destY, $destW, $destH

$g.DrawImage($drill, $destRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)

$g.Dispose()
$bmp.Save($actionOutPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
$bg.Dispose()
$drill.Dispose()

Write-Host "Successfully generated: $actionOutPath"
