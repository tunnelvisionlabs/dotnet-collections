# Note: these values may only change during major release

If ($Version.Contains('-')) {

	# Use the development keys
	$Keys = @{
		'net35-client' = '865cbfcb04071065'
		'net40-client' = '865cbfcb04071065'
		'portable-net40' = '865cbfcb04071065'
		'portable-net45' = '865cbfcb04071065'
	}

} Else {

	# Use the final release keys
	$Keys = @{
		'net35-client' = '8acff7051142f28d'
		'net40-client' = '8acff7051142f28d'
		'portable-net40' = '8acff7051142f28d'
		'portable-net45' = '8acff7051142f28d'
	}

}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
