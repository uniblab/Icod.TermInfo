param(
	[Parameter( Mandatory = $true, Position = 0 )]
	[string] $OutputPath,

	[string] $CanonicalName = 'hdb06-distribution-main',

	[string] $AliasName = 'hdb06-distribution-alias',

	[string] $Description = 'HDB06 distribution terminal'
)

$ErrorActionPreference = 'Stop'

function Set-UInt16LittleEndian {
	param(
		[byte[]] $Buffer,
		[int] $Offset,
		[uint16] $Value
	)

	$Buffer[$Offset] = [byte]( $Value -band 0xff )
	$Buffer[$Offset + 1] = [byte]( ( $Value -shr 8 ) -band 0xff )
}

function Set-UInt32LittleEndian {
	param(
		[byte[]] $Buffer,
		[int] $Offset,
		[uint32] $Value
	)

	for ( $index = 0; $index -lt 4; $index++ ) {
		$Buffer[$Offset + $index] = [byte](
			( $Value -shr ( 8 * $index ) ) -band 0xff
		)
	}
}

function New-MarkedValue {
	param(
		[byte] $Marker,
		[byte[]] $Payload
	)

	$value = [byte[]]::new( $Payload.Length + 1 )
	$value[0] = $Marker
	[Array]::Copy( $Payload, 0, $value, 1, $Payload.Length )
	return ,$value
}

if ( [string]::IsNullOrWhiteSpace( $CanonicalName ) ) {
	throw 'CanonicalName cannot be empty.'
}
if ( [string]::IsNullOrWhiteSpace( $AliasName ) ) {
	throw 'AliasName cannot be empty.'
}
if ( [string]::IsNullOrWhiteSpace( $Description ) ) {
	throw 'Description cannot be empty.'
}

$utf8 = [System.Text.UTF8Encoding]::new( $false )
$latin1 = [System.Text.Encoding]::GetEncoding( 'iso-8859-1' )
$storageName = "$CanonicalName|$AliasName|$Description"
$storageKey = $utf8.GetBytes( $storageName )
$identityBytes = $latin1.GetBytes( "$storageName`0" )
$compiledLength = 12 + $identityBytes.Length
if ( 0 -ne ( $compiledLength -band 1 ) ) {
	$compiledLength++
}
$compiledEntry = [byte[]]::new( $compiledLength )
Set-UInt16LittleEndian $compiledEntry 0 0x011a
Set-UInt16LittleEndian $compiledEntry 2 $identityBytes.Length
[Array]::Copy( $identityBytes, 0, $compiledEntry, 12, $identityBytes.Length )

$records = @(
	[pscustomobject]@{
		Key = $utf8.GetBytes( $CanonicalName )
		Value = New-MarkedValue 2 $storageKey
	},
	[pscustomobject]@{
		Key = $utf8.GetBytes( $AliasName )
		Value = New-MarkedValue 2 $storageKey
	},
	[pscustomobject]@{
		Key = $storageKey
		Value = New-MarkedValue 0 $compiledEntry
	}
)

$pageSize = 512
$database = [byte[]]::new( $pageSize * ( $records.Count + 1 ) )
Set-UInt32LittleEndian $database 12 0x00061561
Set-UInt32LittleEndian $database 16 9
Set-UInt32LittleEndian $database 20 $pageSize
$database[25] = 8
Set-UInt32LittleEndian $database 32 $records.Count

for ( $recordIndex = 0; $recordIndex -lt $records.Count; $recordIndex++ ) {
	$record = $records[$recordIndex]
	$pageOffset = $pageSize * ( $recordIndex + 1 )
	Set-UInt32LittleEndian $database ( $pageOffset + 8 ) ( $recordIndex + 1 )
	$database[$pageOffset + 25] = 13
	$keyOffset = 511 - $record.Key.Length
	$valueOffset = $keyOffset - $record.Value.Length - 1
	if ( $valueOffset -le 30 ) {
		throw 'The controlled HDB06 record does not fit its Hash-v9 page.'
	}
	Set-UInt16LittleEndian $database ( $pageOffset + 20 ) 2
	Set-UInt16LittleEndian $database ( $pageOffset + 22 ) $valueOffset
	Set-UInt16LittleEndian $database ( $pageOffset + 26 ) $keyOffset
	Set-UInt16LittleEndian $database ( $pageOffset + 28 ) $valueOffset
	$database[$pageOffset + $keyOffset] = 1
	[Array]::Copy(
		$record.Key,
		0,
		$database,
		$pageOffset + $keyOffset + 1,
		$record.Key.Length
	)
	$database[$pageOffset + $valueOffset] = 1
	[Array]::Copy(
		$record.Value,
		0,
		$database,
		$pageOffset + $valueOffset + 1,
		$record.Value.Length
	)
}

$fullOutputPath = [System.IO.Path]::GetFullPath( $OutputPath )
$outputDirectory = Split-Path -Parent $fullOutputPath
if ( -not [string]::IsNullOrEmpty( $outputDirectory ) ) {
	[System.IO.Directory]::CreateDirectory( $outputDirectory ) | Out-Null
}
[System.IO.File]::WriteAllBytes( $fullOutputPath, $database )
Write-Host "Created controlled HDB06 Hash-v9 store '$fullOutputPath'."
