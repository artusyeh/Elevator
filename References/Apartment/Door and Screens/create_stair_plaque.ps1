Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap(1024,512)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::FromArgb(191,190,170))
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(42,54,47))
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(105,110,95),5)
$g.DrawRectangle($pen,17,17,990,478)
$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$font = New-Object System.Drawing.Font('Yu Gothic',104,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
$g.DrawString('階 段',$font,$brush,(New-Object System.Drawing.RectangleF(60,68,904,180)),$format)
$fontSmall = New-Object System.Drawing.Font('Yu Gothic',49,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
$g.DrawString('施錠中',$fontSmall,$brush,(New-Object System.Drawing.RectangleF(60,300,904,95)),$format)
$random = New-Object System.Random(31)
for($i=0;$i -lt 2500;$i++){$x=$random.Next(1024);$y=$random.Next(512);$grain=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb($random.Next(4,17),55,64,48));$g.FillRectangle($grain,$x,$y,1,1);$grain.Dispose()}
$bmp.Save('F:/桌面文件/Elevator/Assets/Apartment/Textures/Stairwell Plaque.png',[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$brush.Dispose();$pen.Dispose();$font.Dispose();$fontSmall.Dispose();$format.Dispose()
