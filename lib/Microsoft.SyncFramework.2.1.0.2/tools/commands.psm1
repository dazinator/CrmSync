<#
.SYNOPSIS
    Gets uninstall records from the registry.

.DESCRIPTION
    This function returns information similar to the "Add or remove programs"
    Windows tool. The function normally works much faster and gets some more
    information.

    Another way to get installed products is: Get-WmiObject Win32_Product. But
    this command is usually slow and it returns only products installed by
    Windows Installer.

    x64 notes. 32 bit process: this function does not get installed 64 bit
    products. 64 bit process: this function gets both 32 and 64 bit products.
#>
function Get-Uninstall
{
 #$path = 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
  $path = @(
            'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*'
            'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
           )  
   
   

    # get all data
    Get-ItemProperty $path | ForEach-Object { $_.DisplayName  }
    
    # use only with name and unistall information
    .{process{ if ($_.DisplayName) { $_ } }} |
    # select more or less common subset of properties
    Select-Object DisplayName |
    # and finally sort by name
    Sort-Object DisplayName
}

<#
.SYNOPSIS
    Returns whether a program with the specified display name (case insensitive) is currently installed, by checking for a registry key.

.DESCRIPTION
    This function checks similar information that is displayed by the "Add or remove programs"
    Windows tool. If it finds an entry with a matching case insensitive display name, it returns true.
#>
function Is-Installed($displayname)
{
$found = $false
$(Get-Uninstall | ForEach-Object { 
IF ($_)
{
$b = $displayname.ToLower()
$c = $_.ToLower().StartsWith($b)
IF ($c)
{
$found = $true
}
ELSE
{
}
}
ELSE
{
}
} 
)
return $found 
}

Export-ModuleMember Is-Installed