<#
.DESCRIPTION
	vslint build script, run using cmdlet Invoke-Build from module InvokeBuild
#>
param (
    [string]
	$Version,

	[string]
	$Output = "packages"
)

task Test {
	exec { dotnet test .\test\vslint.Tests\vslint.Tests.fsproj }
}

task AssertOutput {
	if (-not (Test-Path $Output)) {
		mkdir $Output | Out-Null
	}
}

task AssertVersion {
	if (-not $Version) {
		throw "Specify version with -Version parameter"
	}
}

task BuildStandalone {
	exec { dotnet build .\src\vslint\vslint.fsproj --configuration Release /p:Version=$Version }
}

task PackageStandalone BuildStandalone, {
	exec { nuget pack vslint.nuspec -OutputDirectory (Resolve-Path $Output) -Version $Version }
}

task PackageTool {
	exec { dotnet pack .\src\dotnet-vslint\dotnet-vslint.fsproj --configuration Release --output (Resolve-Path $Output) /p:Version=$Version }
}

task . AssertVersion, AssertOutput, Test, PackageStandalone, PackageTool
