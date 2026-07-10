param([string]$OutFile = "shot.png")
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
$bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
$bmp = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($bounds.X, $bounds.Y, 0, 0, $bmp.Size)
$g.Dispose()
# scale down to keep files reasonable (3440-wide -> 1376)
$scale = 1376 / $bmp.Width
$small = New-Object System.Drawing.Bitmap ([int]($bmp.Width * $scale)), ([int]($bmp.Height * $scale))
$g2 = [System.Drawing.Graphics]::FromImage($small)
$g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g2.DrawImage($bmp, 0, 0, $small.Width, $small.Height)
$g2.Dispose()
$small.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose(); $small.Dispose()
Write-Output "saved: $OutFile"
