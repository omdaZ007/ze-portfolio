# Extracts ZE visual assets from the supplied reference sheets.
# Coordinates are fractions of the source image (x0, y0, x1, y1).
Add-Type -AssemblyName System.Drawing

$refs = 'E:\ZE\docs\references'
$outImages = 'E:\ZE\wwwroot\images'
$outFounders = 'E:\ZE\wwwroot\uploads\founders'
$outProjects = 'E:\ZE\wwwroot\uploads\projects'
New-Item -ItemType Directory -Force -Path $outImages, $outFounders, $outProjects | Out-Null

$assets = 'E:\ZE\docs\references\02-ze-assets.png.jpg'
$founders = 'E:\ZE\docs\references\03-founders-reference.png.jpg'
$homeRef = 'E:\ZE\docs\references\01-homepage-reference.png.jpg'

$specs = @(
    # --- ZE branding / hero visuals (asset sheet) ---
    @{ n = 'ze-hero-logo';    s = $assets;  x0 = .028; y0 = .030; x1 = .372; y1 = .455 },
    @{ n = 'ze-nav-logo';     s = $founders; x0 = .064; y0 = .012; x1 = .156; y1 = .078 },
    @{ n = 'island-castle';   s = $assets;  x0 = .018; y0 = .440; x1 = .245; y1 = .670 },
    @{ n = 'islands-small';   s = $assets;  x0 = .240; y0 = .525; x1 = .472; y1 = .655 },
    @{ n = 'island-cloud';    s = $assets;  x0 = .795; y0 = .825; x1 = .995; y1 = .995 },
    @{ n = 'laptop-code';     s = $assets;  x0 = .425; y0 = .075; x1 = .775; y1 = .440 },
    @{ n = 'code-window';     s = $assets;  x0 = .738; y0 = .075; x1 = .968; y1 = .400 },
    @{ n = 'browser-window';  s = $assets;  x0 = .535; y0 = .445; x1 = .792; y1 = .678 },
    @{ n = 'glass-cards';     s = $assets;  x0 = .802; y0 = .445; x1 = .978; y1 = .672 },
    @{ n = 'orbit';           s = $assets;  x0 = .022; y0 = .822; x1 = .178; y1 = .992 },
    @{ n = 'globe';           s = $assets;  x0 = .182; y0 = .822; x1 = .298; y1 = .995 },
    @{ n = 'feature-card';    s = $assets;  x0 = .312; y0 = .822; x1 = .448; y1 = .995 },
    @{ n = 'cards-stack';     s = $assets;  x0 = .462; y0 = .822; x1 = .650; y1 = .995 },
    @{ n = 'plant';           s = $assets;  x0 = .658; y0 = .825; x1 = .792; y1 = .995 },

    # --- Tech icon squares (asset sheet, row) ---
    @{ n = 'icon-html5';      s = $assets;  x0 = .045; y0 = .700; x1 = .102; y1 = .784 },
    @{ n = 'icon-css3';       s = $assets;  x0 = .108; y0 = .700; x1 = .165; y1 = .784 },
    @{ n = 'icon-js';         s = $assets;  x0 = .172; y0 = .700; x1 = .229; y1 = .784 },
    @{ n = 'icon-bootstrap';  s = $assets;  x0 = .236; y0 = .700; x1 = .293; y1 = .784 },
    @{ n = 'icon-react';      s = $assets;  x0 = .300; y0 = .700; x1 = .357; y1 = .784 },
    @{ n = 'icon-tailwind';   s = $assets;  x0 = .364; y0 = .700; x1 = .421; y1 = .784 },
    @{ n = 'icon-git';        s = $assets;  x0 = .430; y0 = .700; x1 = .487; y1 = .784 },
    @{ n = 'icon-github';     s = $assets;  x0 = .494; y0 = .700; x1 = .551; y1 = .784 },
    @{ n = 'icon-vscode';     s = $assets;  x0 = .558; y0 = .700; x1 = .615; y1 = .784 },
    @{ n = 'icon-figma';      s = $assets;  x0 = .622; y0 = .700; x1 = .679; y1 = .784 },

    # --- Founder photos (supplied photos, framed) ---
    @{ n = 'ziad';     s = $founders; x0 = .019; y0 = .293; x1 = .269; y1 = .702; dir = $outFounders },
    @{ n = 'mohamed';  s = $founders; x0 = .396; y0 = .312; x1 = .615; y1 = .730; dir = $outFounders },

    # --- About visual (homepage reference, laptop in organic blob) ---
    @{ n = 'about-laptop'; s = $homeRef; x0 = .095; y0 = .600; x1 = .310; y1 = .885 }
)

# Seed project images (distinct visuals from the asset sheet)
$projectImages = @(
    @{ n = 'proj-ze-platform'; s = $assets; x0 = .028; y0 = .030; x1 = .372; y1 = .455 },
    @{ n = 'proj-dashboard';   s = $assets; x0 = .535; y0 = .445; x1 = .792; y1 = .678 },
    @{ n = 'proj-code-editor'; s = $assets; x0 = .425; y0 = .075; x1 = .775; y1 = .440 },
    @{ n = 'proj-api-console'; s = $assets; x0 = .738; y0 = .075; x1 = .968; y1 = .400 },
    # --- Project images (distinct visuals from the asset sheet) ---
    @{ n = 'proj-ui-kit';      s = $assets; x0 = .462; y0 = .822; x1 = .650; y1 = .995 },
    @{ n = 'proj-portal';      s = $assets; x0 = .802; y0 = .445; x1 = .978; y1 = .672 }
)

function Crop-Asset($spec, $destDir) {
    $img = [System.Drawing.Image]::FromFile($spec.s)
    try {
        $x = [int]($spec.x0 * $img.Width)
        $y = [int]($spec.y0 * $img.Height)
        $w = [int](($spec.x1 - $spec.x0) * $img.Width)
        $h = [int](($spec.y1 - $spec.y0) * $img.Height)
        if ($x -lt 0) { $x = 0 }
        if ($y -lt 0) { $y = 0 }
        if ($x + $w -gt $img.Width) { $w = $img.Width - $x }
        if ($y + $h -gt $img.Height) { $h = $img.Height - $y }
        $rect = New-Object System.Drawing.Rectangle($x, $y, $w, $h)
        $bmp = New-Object System.Drawing.Bitmap($w, $h)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.DrawImage($img, (New-Object System.Drawing.Rectangle(0, 0, $w, $h)), $rect, [System.Drawing.GraphicsUnit]::Pixel)
        $g.Dispose()
        $path = Join-Path $destDir ($spec.n + '.png')
        $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()
        Write-Output ("{0} -> {1}x{2}" -f $spec.n, $w, $h)
    }
    finally { $img.Dispose() }
}

foreach ($s in $specs) {
    $dir = if ($s.ContainsKey('dir')) { $s.dir } else { $outImages }
    Crop-Asset $s $dir
}
foreach ($s in $projectImages) { Crop-Asset $s $outProjects }

# Create a favicon-friendly square logo crop + a scaled 64px version
Write-Output 'done'
