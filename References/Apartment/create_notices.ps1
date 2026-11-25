Add-Type -AssemblyName System.Drawing
$target = 'F:/桌面文件/Elevator/Assets/Apartment/Textures'
$font = New-Object System.Drawing.Font('Microsoft YaHei',28)
$small = New-Object System.Drawing.Font('Microsoft YaHei',18)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(65,60,43))
$docs = @{
'Building Notices' = @('青葉荘　掲示板','お知らせ','共用廊下に物を置かないでください。','ごみ収集日：火・金曜日　午前８時まで','夜１０時以降はお静かにお願いします。','設備の故障は管理室までご連絡ください。','防犯カメラ作動中','管理室　受付時間　９：００～１８：００');
'Inspection Certificate' = @('昇降機 定期検査済証','青葉荘　乗用エレベーター','定員　６名　　積載量　４５０kg','使用階　１階 ～ ６階','点検日　２０２６年１１月１０日','異常を感じた場合は使用を中止し、','管理室へ連絡してください。','緊急連絡先　管理室');
'Maintenance Log' = @('青葉荘　設備管理日誌','日付　　 点検項目　　　　　 確認','１１／２４　照明・共用廊下　　 済','１１／２４　給水設備　　　　　 済','１１／２４　エレベーター　　　 済','','備考：３階の蛍光灯を交換する。','担当者　＿＿＿＿＿＿＿＿＿＿');
'Calendar' = @('２０２６年　１１月','日　月　火　水　木　金　土','１　２　３　４　５　６　７','８　９　10　11　12　13　14','15　16　17　18　19　20　21','22　23　24　25　26　27　28','29　30','','設備点検　毎月１０日');
'Building Plan' = @('青葉荘　共用部案内図','１階　管理室・郵便受け・エントランス','２階 ～ ６階　住戸・共用廊下','','［ 階段 ］　　［ 共用廊下 ］　［ 昇降機 ］','','非常時は階段を使用してください。')
}
foreach($name in $docs.Keys){
 $bmp = New-Object System.Drawing.Bitmap(900,1100)
 $g = [System.Drawing.Graphics]::FromImage($bmp)
 $g.Clear([System.Drawing.Color]::FromArgb(190,180,145))
 $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
 $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(104,95,67),3)
 $g.DrawRectangle($pen,25,25,850,1050)
 $line = 0
 foreach($text in $docs[$name]){$g.DrawString($text,$(if($line -eq 0){$font}else{$small}),$brush,55,(65+$line*85));$line++}
 for($i=0;$i -lt 8;$i++){$g.DrawLine($pen,55,165+$i*85,845,165+$i*85)}
 $bmp.Save("$target/$name.png",[System.Drawing.Imaging.ImageFormat]::Png)
 $g.Dispose();$bmp.Dispose();$pen.Dispose()
}
$font.Dispose();$small.Dispose();$brush.Dispose()
