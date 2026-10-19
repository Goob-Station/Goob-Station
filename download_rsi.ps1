$json = Invoke-RestMethod -Uri "https://api.github.com/repos/funky-station/funky-station/contents/Resources/Textures/Interface/Actions/actions_malf_ai.rsi?ref=refs/pull/1493/head"
$outDir = "Resources/Textures/Interface/Actions/actions_malf_ai.rsi"
New-Item -ItemType Directory -Force -Path $outDir

foreach ($file in $json) {
    $outFile = "$outDir/$($file.name)"
    Write-Host "Downloading $($file.name)..."
    Invoke-WebRequest -Uri $file.download_url -OutFile $outFile
}

Write-Host "Done! Downloaded RSI icons."
