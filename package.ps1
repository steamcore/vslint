param (
    [Parameter(Mandatory=$True)]
    [string]
    $Version
)

function Assert-SuccessfulExitCode {
    if ($LastExitCode -ne 0) {
        Write-Error "Build failed with code $LastExitCode"
        throw ("Build failed with code " + $LastExitCode)
    }
}

function Enter-Task($message, $action) {
    Write-Host $message -ForegroundColor Yellow
    & $action
    Assert-SuccessfulExitCode
}

Enter-Task "Test" {
	dotnet test .\vslint.Tests\vslint.Tests.fsproj
}

Enter-Task "Build" {
	dotnet build .\vslint\vslint.fsproj --configuration Release /p:Version=$Version
}

Enter-Task "Package" {
	nuget pack vslint.nuspec -Version $Version
}
