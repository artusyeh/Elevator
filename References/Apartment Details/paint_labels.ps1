Add-Type -AssemblyName System.Drawing
$textureRoot=Join-Path $PSScriptRoot '../../Assets/Building/Textures/Common Areas'
New-Item -ItemType Directory -Force -Path $textureRoot | Out-Null
function Paint-Paper($name,$text) {
    $bitmap=New-Object System.Drawing.Bitmap 256,1024
    $graphics=[System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode='AntiAlias'
    $rng=[System.Random]::new(37+$name.Length)
    for($y=5;$y -lt 1019;$y++) {
        $left=7+$rng.Next(0,5); $right=248-$rng.Next(0,5)
        $shade=$rng.Next(-7,8)
        $pen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,123+$shade,40+$shade,30+$shade))
        $graphics.DrawLine($pen,$left,$y,$right,$y);$pen.Dispose()
    }
    $font=[System.Drawing.Font]::new('Microsoft YaHei',104,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
    $brush=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(240,185,146,76))
    $format=[System.Drawing.StringFormat]::new();$format.Alignment='Center';$format.LineAlignment='Center'
    for($i=0;$i -lt $text.Length;$i++) { $graphics.DrawString($text.Substring($i,1),$font,$brush,[System.Drawing.RectangleF]::new(12,32+$i*134,232,135),$format) }
    for($i=0;$i -lt 1400;$i++) { $x=$rng.Next(12,244);$y=$rng.Next(10,1014);$dust=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb($rng.Next(12,65),67,56,34));$graphics.FillEllipse($dust,$x,$y,$rng.Next(1,5),$rng.Next(1,6));$dust.Dispose() }
    $bitmap.Save((Join-Path $textureRoot ($name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
    $font.Dispose();$brush.Dispose();$format.Dispose();$graphics.Dispose();$bitmap.Dispose()
}
Paint-Paper 'Couplet Left' '一帆風順年年好'
Paint-Paper 'Couplet Right' '萬事如意步步高'
$bitmap=New-Object System.Drawing.Bitmap 768,384
$g=[System.Drawing.Graphics]::FromImage($bitmap);$g.Clear([System.Drawing.Color]::FromArgb(193,188,163));$g.TextRenderingHint='AntiAliasGridFit'
$ink=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(49,48,42));$font=[System.Drawing.Font]::new('Microsoft YaHei',52)
$g.DrawString('青葉だより',$font,$ink,30,12)
$pen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(99,96,81),2)
for($c=0;$c -lt 3;$c++) { for($row=0;$row -lt 16;$row++) {$g.DrawLine($pen,30+$c*244,105+$row*15,220+$c*244,105+$row*15)} }
$bitmap.Save((Join-Path $textureRoot 'Neighborhood Paper.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bitmap.Dispose();$font.Dispose();$ink.Dispose();$pen.Dispose()
$bitmap=New-Object System.Drawing.Bitmap 768,256
$g=[System.Drawing.Graphics]::FromImage($bitmap);$g.Clear([System.Drawing.Color]::FromArgb(36,76,57));$g.TextRenderingHint='AntiAliasGridFit'
$white=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(208,213,184));$font=[System.Drawing.Font]::new('Microsoft YaHei',87)
$g.DrawString('非常口',$font,$white,50,48)
$points=[System.Drawing.Point[]]@([System.Drawing.Point]::new(554,128),[System.Drawing.Point]::new(633,49),[System.Drawing.Point]::new(633,94),[System.Drawing.Point]::new(715,94),[System.Drawing.Point]::new(715,162),[System.Drawing.Point]::new(633,162),[System.Drawing.Point]::new(633,207))
$g.FillPolygon($white,$points)
$bitmap.Save((Join-Path $textureRoot 'Emergency Exit.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bitmap.Dispose();$font.Dispose();$white.Dispose()
