# Builds a contact sheet of extracted crops for visual verification.
Add-Type -AssemblyName System.Drawing
$files = @(
    'E:\ZE\wwwroot\uploads\founders\ziad.png',
    'E:\ZE\wwwroot\uploads\founders\mohamed.png',
    'E:\ZE\wwwroot\images\ze-hero-logo.png',
    'E:\ZE\wwwroot\images\ze-nav-logo.png',
    'E:\ZE\wwwroot\images\about-laptop.png',
    'E:\ZE\wwwroot\images\laptop-code.png',
    'E:\ZE\wwwroot\images\code-window.png',
    'E:\ZE\wwwroot\images\browser-window.png',
    'E:\ZE\wwwroot\images\glass-cards.png',
    'E:\ZE\wwwroot\images\island-castle.png',
    'E:\ZE\wwwroot\images\islands-small.png',
    'E:\ZE\wwwroot\images\island-cloud.png',
    'E:\ZE\wwwroot\images\icon-html5.png',
    'E:\ZE\wwwroot\images\icon-css3.png',
    'E:\ZE\wwwroot\images\icon-js.png',
    'E:\ZE\wwwroot\images\icon-bootstrap.png',
    'E:\ZE\wwwroot\images\icon-react.png',
    'E:\ZE\wwwroot\images\icon-tailwind.png',
    'E:\ZE\wwwroot\images\icon-git.png',
    'E:\ZE\wwwroot\images\icon-github.png',
    'E:\ZE\wwwroot\images\icon-vscode.png',
    'E:\ZE\wwwroot\images\icon-figma.png',
    'E:\ZE\wwwroot\images\orbit.png',
    'E:\ZE\wwwroot\images\globe.png',
    'E:\ZE\wwwroot\images\cards-stack.png',
    'E:\ZE\wwwroot\images\feature-card.png',
    'E:\ZE\wwwroot\images\plant.png'
)
$cols = 6
$cellW = 220; $cellH = 220
$rows = [math]::Ceiling($files.Count / $cols)
$bmp = New-Object System.Drawing.Bitmap(($cols * $cellW), ($rows * $cellH))
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::FromArgb(255, 7, 17, 31))
$font = New-Object System.Drawing.Font('Consolas', 9)
$brush = [System.Drawing.Brushes]::White
for ($i = 0; $i -lt $files.Count; $i++) {
    if (-not (Test-Path $files[$i])) { continue }
    $img = [System.Drawing.Image]::FromFile($files[$i])
    $scale = [math]::Min(($cellW - 20) / $img.Width, ($cellH - 40) / $img.Height)
    $w = [int]($img.Width * $scale); $h = [int]($img.Height * $scale)
    $cx = ($i % $cols) * $cellW; $cy = [math]::Floor($i / $cols) * $cellH
    $g.DrawImage($img, ($cx + 10), ($cy + 10), $w, $h)
    $name = [System.IO.Path]::GetFileNameWithoutExtension($files[$i])
    $g.DrawString($name, $font, $brush, ($cx + 8), ($cy + $cellH - 26))
    $img.Dispose()
}
$g.Dispose()
$bmp.Save('E:\ZE\docs\contact-sheet.jpg', [System.Drawing.Imaging.ImageFormat]::Jpeg)
$bmp.Dispose()
Write-Output 'contact sheet saved'
