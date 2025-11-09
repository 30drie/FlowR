$scriptName = $MyInvocation.MyCommand.Name
$artifacts = "./artifacts"

if ([string]::IsNullOrEmpty($Env:NUGET_API_KEY)) {
    Write-Host "${scriptName}: NUGET_API_KEY is empty or not set. Skipped pushing package(s)."
} else {
    # Push primary packages
    Get-ChildItem $artifacts -Filter "*.nupkg" | ForEach-Object {
        Write-Host "$($scriptName): Pushing $($_.Name)"
        dotnet nuget push $_ --source $Env:NUGET_URL --api-key $Env:NUGET_API_KEY --skip-duplicate
        if ($lastexitcode -ne 0) {
            throw ("Exec: " + $errorMessage)
        }
    }

    # Push symbol packages if present
    Get-ChildItem $artifacts -Filter "*.snupkg" | ForEach-Object {
        Write-Host "$($scriptName): Pushing symbols $($_.Name)"
        dotnet nuget push $_ --source $Env:NUGET_URL --api-key $Env:NUGET_API_KEY --skip-duplicate
        if ($lastexitcode -ne 0) {
            throw ("Exec: " + $errorMessage)
        }
    }
}
