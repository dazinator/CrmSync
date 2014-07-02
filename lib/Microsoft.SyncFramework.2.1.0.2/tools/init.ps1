param($installPath, $toolsPath, $package, $project)

Import-Module (Join-Path $toolsPath commands.psm1)

# Required Sync Framework Redistributables.
$requiredRedistributables = @{
  "microsoft sync framework 2.1 core components (x86) enu" = "x86\Synchronization-v2.1-x86-ENU.msi" 
  "microsoft sync framework 2.1 core components (x64) enu" = "x64\Synchronization-v2.1-x64-ENU.msi" 
 }

 # Ensure redistributables are installed.
foreach ($redistributable in $requiredRedistributables.GetEnumerator()) {
    Write-Host "Checking whether $($redistributable.Name) is installed. "
	$isInstalled = $(Is-Installed($redistributable.Name))

	if(!$isInstalled)
	{
	    # TODO: Only run x64 bit version if current environment processor architecture is 64 bit?
		# Run MSI
		$msifilename = join-path $toolsPath "$($redistributable.Value)"
		Write-Host "Installing $($redistributable.Name) from path: $($msifilename)"
		Start-Process -FilePath "$env:systemroot\system32\msiexec.exe" -ArgumentList "/i `"$msifilename`" /qn /passive" -Wait 
	}

}