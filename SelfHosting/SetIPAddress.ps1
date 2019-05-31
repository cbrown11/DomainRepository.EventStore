$ipV4= Get-WmiObject -Class Win32_PingStatus -Filter "Address='$ComputerName' AND Timeout=1000" | Select -ExpandProperty IPV4Address;

if(!$ipV4)
{
	$ipV4 = Test-Connection -ComputerName (hostname) -Count 1  | Select -ExpandProperty IPV4Address
}
Write-Host $ipV4.IPAddressToString
[Environment]::SetEnvironmentVariable("EVENTSTORE_INT_IP", $ipV4.IPAddressToString, "User")
[Environment]::SetEnvironmentVariable("EVENTSTORE_EXT_IP", $ipV4.IPAddressToString, "User")