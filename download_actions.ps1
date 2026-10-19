$files = @(
    "Content.Shared/Actions/Events/ActionPurchaseCompanionEvent.cs",
    "Content.Shared/Actions/Events/MalfAiGyroscopeActionEvent.cs",
    "Content.Shared/Actions/Events/MalfAiOverloadMachineActionEvent.cs",
    "Content.Shared/Actions/Events/MalfAiRoboticsFactoryActionEvent.cs",
    "Content.Shared/Actions/Events/MalfAiVoiceModulatorActionEvent.cs"
)
$baseUrl = "https://raw.githubusercontent.com/funky-station/funky-station/refs/pull/1493/head"
foreach ($file in $files) {
    Invoke-WebRequest -Uri "$baseUrl/$file" -OutFile $file
}
Write-Host "Downloaded action events"
