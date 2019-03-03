<#
.DESCRIPTION
	vslint build script, run using cmdlet Invoke-Build from module InvokeBuild
#>
param (
    [string]
    $Version
)

task Test {
	exec { dotnet test .\vslint.Tests\vslint.Tests.fsproj }
}

task AssertVersion {
	if (-not $Version) {
		throw "Specify version with -Version parameter"
	}
}

task Build {
	exec { dotnet build .\vslint\vslint.fsproj --configuration Release /p:Version=$Version }
}

task Package Build, {
	exec { nuget pack vslint.nuspec -Version $Version }
}

task . AssertVersion, Test, Package
