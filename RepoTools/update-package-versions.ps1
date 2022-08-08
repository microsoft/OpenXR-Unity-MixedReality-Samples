# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

$repoRoot = Join-Path $PSScriptRoot ".."

Get-ChildItem $repoRoot/MixedRealityPackages/*.tgz | ForEach-Object {
    $fileName = $_.Name
    $packageName = (Split-Path $_ -Leaf).Split('-')
    $packageVersion = $packageName[($packageName.Length - 1)..($packageName.Length)].Split('.')
    $packageVersion = $packageVersion[0..($packageVersion.Length - 2)] -join '.'
    $packageName = $packageName[0..($packageName.Length - 2)] -join '-'
    Write-Host "Checking for $packageName version $packageVersion..." -ForegroundColor Green

    Get-ChildItem $repoRoot/*/Packages/manifest.json | ForEach-Object {
        $content = Get-Content $_ -Raw
        if ($content -match $packageName -and $content -notmatch $fileName) {
            Write-Host "Updating $($_.Directory.Parent.Name)"
            ($content -replace "`"$packageName`": `"file:../../MixedRealityPackages/$packageName-[\w.]+.tgz`"", "`"$packageName`": `"file:../../MixedRealityPackages/$packageName-$packageVersion.tgz`"") | Set-Content -Path $_  -NoNewline
        }
    }
}
