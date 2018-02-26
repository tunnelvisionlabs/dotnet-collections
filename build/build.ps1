param (
	[switch]$Debug,
	[string]$VisualStudioVersion = "15.0",
	[switch]$NoClean,
	[string]$Verbosity = "minimal",
	[string]$Logger,
	[switch]$SkipKeyCheck
)

# build the solution
$SolutionPath = "..\CollectionsCompat.sln"

# make sure the script was run from the expected path
if (!(Test-Path $SolutionPath)) {
	$host.ui.WriteErrorLine('The script was run from an invalid working directory.')
	exit 1
}

. .\version.ps1

If ($Debug) {
	$BuildConfig = 'Debug'
} Else {
	$BuildConfig = 'Release'
}

If ($Version.Contains('-')) {
	$KeyConfiguration = 'Dev'
} Else {
	$KeyConfiguration = 'Final'
}

If ($NoClean) {
	$Target = 'build'
} Else {
	$Target = 'rebuild'
}

# build the main project
$nuget = '..\.nuget\NuGet.exe'
If (-not (Test-Path $nuget)) {
	If (-not (Test-Path '..\.nuget')) {
		mkdir '..\.nuget'
	}

	$nugetSource = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
	Invoke-WebRequest $nugetSource -OutFile $nuget
	If (-not $?) {
		$host.ui.WriteErrorLine('Unable to download NuGet executable, aborting!')
		exit $LASTEXITCODE
	}
}

$visualStudio = (Get-ItemProperty 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7')."$VisualStudioVersion"
$msbuild = "$visualStudio\MSBuild\$VisualStudioVersion\Bin\MSBuild.exe"
If (-not (Test-Path $msbuild)) {
	$host.UI.WriteErrorLine("Couldn't find MSBuild.exe")
	exit 1
}

&$nuget 'restore' $SolutionPath -Project2ProjectTimeOut 1200
&$msbuild '/nologo' '/m' '/nr:false' "/t:$Target" $LoggerArgument "/verbosity:$Verbosity" "/p:Configuration=$BuildConfig" "/p:VisualStudioVersion=$VisualStudioVersion" "/p:KeyConfiguration=$KeyConfiguration" $SolutionPath
if (-not $?) {
	$host.ui.WriteErrorLine('Build failed, aborting!')
	Exit $LASTEXITCODE
}

# By default, do not create a NuGet package unless the expected strong name key files were used
if (-not $SkipKeyCheck) {
	. .\keys.ps1

	foreach ($pair in $Keys.GetEnumerator()) {
		$assembly = Resolve-FullPath -Path "..\System.Collections.Immutable\src\bin\$BuildConfig\$($pair.Key)\Rackspace.Collections.Immutable.dll"
		# Run the actual check in a separate process or the current process will keep the assembly file locked
		powershell -Command ".\check-key.ps1 -Assembly '$assembly' -ExpectedKey '$($pair.Value)' -Build '$($pair.Key)'"
		if (-not $?) {
			Exit $LASTEXITCODE
		}
	}
}

if (-not (Test-Path 'nuget')) {
	mkdir "nuget"
}

Copy-Item "..\System.Collections.Immutable\src\bin\$BuildConfig\Rackspace.Collections.Immutable.$Version.nupkg" 'nuget'
Copy-Item "..\System.Collections.Immutable\src\bin\$BuildConfig\Rackspace.Collections.Immutable.$Version.symbols.nupkg" 'nuget'
