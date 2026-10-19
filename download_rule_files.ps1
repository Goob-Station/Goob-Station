$files = @(
    "Content.Server/GameTicking/Rules/MalfAiRuleSystem.cs",
    "Content.Server/GameTicking/Rules/Components/MalfAIRuleComponent.cs",
    "Content.Server/Roles/MalfAiRoleComponent.cs",
    "Resources/Prototypes/Roles/Antags/malfunctioning_ai.yml",
    "Content.Shared/MalfAI/SharedMalfAiHelpers.cs"
)

$baseUrl = "https://raw.githubusercontent.com/funky-station/funky-station/refs/pull/1493/head"

foreach ($file in $files) {
    $targetDir = Split-Path -Path $file -Parent
    if (-not (Test-Path -Path $targetDir)) {
        New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
    }
    Write-Host "Downloading $file..."
    Invoke-WebRequest -Uri "$baseUrl/$file" -OutFile $file
}
Write-Host "Done!"
