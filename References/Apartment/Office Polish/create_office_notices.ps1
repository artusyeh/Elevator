Add-Type -AssemblyName System.Drawing
$folder = 'F:/桌面文件/Elevator/Assets/Apartment/Textures'
$documents = @{
 'Caretaker Notes' = @('夜間 引継ぎメモ','照明：３階の蛍光灯を交換','給水ポンプの異音を確認','返却された鍵は台帳へ記入','巡回後は共用窓を確認','次回点検：１２月１０日','管理室　青葉荘');
 'Supply Label' = @('保守用品　予備部品');
 'Archive Label' = @('点検記録・設備資料');
 'Key Return' = @('返却鍵はこちら')
}
foreach($name in $documents.Keys){
 $isLabel = $name -ne 'Caretaker Notes'
 $width=700;$height=960
 if($isLabel){$height=180}
 $bmp=New-Object System.Drawing.Bitmap($width,$height)
 $g=[System.Drawing.Graphics]::FromImage($bmp)
 $g.Clear([System.Drawing.Color]::FromArgb(186,177,146))
 $g.TextRenderingHint=[System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
 $font=New-Object System.Drawing.Font('Microsoft YaHei',$(if($isLabel){38}else{26}))
 $brush=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(53,57,44))
 $pen=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(104,101,81),2)
 $g.DrawRectangle($pen,17,17,($width-34),($height-34))
 $line=0
 foreach($text in $documents[$name]){$g.DrawString($text,$font,$brush,35,(48+$line*115));$line++}
 $bmp.Save("$folder/$name.png",[System.Drawing.Imaging.ImageFormat]::Png)
 $font.Dispose();$brush.Dispose();$pen.Dispose();$g.Dispose();$bmp.Dispose()
}
# Replace the text-only sheet with a small diagram of the actual ground-floor organization.
$bmp=New-Object System.Drawing.Bitmap(1200,600)
$g=[System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::FromArgb(188,181,153))
$pen=New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(49,59,48),5)
$font=New-Object System.Drawing.Font('Microsoft YaHei',22)
$brush=[System.Drawing.Brushes]::Black
$g.DrawString('青葉荘　１階 共用部案内',$font,$brush,35,25)
$g.DrawRectangle($pen,100,120,295,280)
$g.DrawRectangle($pen,395,120,320,390)
$g.DrawRectangle($pen,100,400,295,110)
$g.DrawRectangle($pen,715,345,180,165)
$g.DrawString('管理室',$font,$brush,180,220)
$g.DrawString('階段',$font,$brush,190,435)
$g.DrawString('共用ロビー',$font,$brush,450,235)
$g.DrawString('昇降機',$font,$brush,740,400)
$g.DrawString('正面入口',$font,$brush,450,95)
$g.DrawString('郵便受け',$font,$brush,725,225)
$g.DrawLine($pen,430,340,650,340)
$g.DrawString('避難経路は掲示をご確認ください。',$font,$brush,100,545)
$bmp.Save("$folder/Building Plan.png",[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$font.Dispose();$pen.Dispose()
