Add-Type -AssemblyName System.Drawing
$folder = 'F:/桌面文件/Elevator/References/Apartment/Captures'
$names = @('Entering','Entrance','Reception','Management Room','Office Desk','Elevator Lobby','Elevator Cabin','Stairwell','Residential Corridor')
$canvas = New-Object System.Drawing.Bitmap(2400,1620)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$graphics.Clear([System.Drawing.Color]::FromArgb(20,22,20))
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$font = New-Object System.Drawing.Font('Arial',20)
for($i=0;$i -lt $names.Length;$i++){
 $photo = [System.Drawing.Image]::FromFile("$folder/$($names[$i]).png")
 $galleryX = ($i % 3)*800
 $galleryY = [math]::Floor($i/3)*540
 $graphics.DrawImage($photo,[int]$galleryX,[int]$galleryY,800,500)
 $graphics.DrawString($names[$i],$font,[System.Drawing.Brushes]::White,[single]($galleryX+12),[single]($galleryY+505))
 $photo.Dispose()
}
$canvas.Save("$folder/Apartment Review.png",[System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose();$canvas.Dispose();$font.Dispose()
