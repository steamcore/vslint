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

task DotnetRestore {
    exec {
		dotnet restore
	}
}

task DotnetBuild DotnetRestore, {
    exec {
		dotnet build --no-restore
	}
}

task DotnetTest {
	exec {
		dotnet test .\test\vslint.Tests\vslint.Tests.fsproj
	}
}

task PackageTool AssertVersion, AssertOutput, {
	exec {
		dotnet pack .\src\dotnet-vslint\dotnet-vslint.fsproj --configuration Release --output (Resolve-Path $Output) /p:Version=$Version
	}
}

task Package PackageTool

task . DotnetBuild, DotnetTest
