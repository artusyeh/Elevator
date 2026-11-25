param([Parameter(Mandatory=$true)][string]$Method)
$workFolder=Join-Path (Get-Location) 'Library/CabinWork'
Remove-Item -LiteralPath (Join-Path $workFolder 'result.txt') -ErrorAction SilentlyContinue
[System.IO.File]::WriteAllText((Join-Path $workFolder 'request.txt'),$Method,[System.Text.UTF8Encoding]::new($false))
for($attempt=0;$attempt -lt 35;$attempt++) {
    if(Test-Path -LiteralPath (Join-Path $workFolder 'result.txt')) { Get-Content -LiteralPath (Join-Path $workFolder 'result.txt'); exit }
    Start-Sleep -Milliseconds 400
}
Write-Output 'Editor command is still pending; inspect Library/CabinWork/result.txt.'
