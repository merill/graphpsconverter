#Synposis: The scripts in this file are used to export all the parameters from the Azure AD and MSOL module as well as load the module names for Graph commands.

#Import-Module Microsoft.Graph #Takes time so load it once.



cd 'F:\Code\graphpsconverter\src\GraphPsConverter\GraphPsConverter.Core\Data\'
$ignoreVariables = @("PipelineVariable","OutBuffer", "OutVariable", "InformationVariable", "InformationVariable", "WarningVariable", "ErrorInformationVariable", "Confirm", "WhatIf", "Verbose", "Debug", "ErrorVariable", "InformationAction", "WarningAction", "ErrorAction")

Function UpdateGraphCommandModuleNameAndScope {
	$isBeta = (Get-MgProfile).Name -eq 'beta';
	$isV1 = !$isBeta;

	$allCmdMapHash.GetEnumerator() | foreach{ 
		$graphCmdName = $_.Value.GraphCmdName; 
		if($graphCmdName -ne '' -and $_.Value.GraphCmdName -ne $null){ 
			#Populate Graph Module name
			$graphCmd = Get-Command $graphCmdName -ErrorAction SilentlyContinue; 
			if ( $graphCmd -eq $null ) {} 
			else {$_.Value.GraphModuleName=$graphCmd.Source; $_.Value.IsV1 = $isV1; $_.Value.IsBeta = $isBeta} 

			#Populate Graph scope
			$graphCmd = Find-MgGraphCommand -Command $graphCmdName -ErrorAction SilentlyContinue; 
			if ( $graphCmd -eq $null ) {} 
			else {
				$_.Value.GraphCmdScope=($graphCmd[0].Permissions | select -ExpandProperty Name) -join ";"; 
				$_.Value.GraphUri=($graphCmd[0].Method + " " + $graphCmd[0].URI)
				$_.Value.IsV1 = $isV1; $_.Value.IsBeta = $isBeta
			} 
		}
	}
}

$cmds = @{}

#Load all commands from AAD and MSol
Remove-Module AzureADPreview
Import-Module AzureAD
Import-Module MSOnline
Get-Command -Module MSOnline, AzureAD | foreach { $cmds[$_.Name.ToLower()] = $_ }

#Load commands unique to aad preview
Remove-Module AzureAD
Import-Module AzureADPreview
Get-Command -Module AzureADPreview | foreach { if(-not $cmds.ContainsKey($_.Name.ToLower())) { $cmds[$_.Name.ToLower()] = $_ } }

#Load all commands
$params = $cmds.GetEnumerator() | foreach{ $cmd = $_.Value.Name; $module = $_.Value.Source; [pscustomobject]@{AadCmdName=$cmd; AadModuleName = $module; GraphCmdName=''; GraphModuleName=''; AadParamName=''; GraphParamName=''; GraphCmdScope=''; GraphUri='';IsV1 = $false; IsBeta = $false}; if($_.Value.Parameters -ne $null){ $_.Value.Parameters.GetEnumerator() | foreach { $paramName = $_.Value.Name; if(-not $ignoreVariables.Contains($paramName)) {[pscustomobject]@{AadCmdName=$cmd; AadModuleName = $module; GraphCmdName=''; GraphModuleName=''; AadParamName=$paramName; GraphParamName='';GraphCmdScope=''; GraphUri='';IsV1 = $false; IsBeta = $false} }} } }

#Read all objects from Azure AD
$allCmdMap = ($params | foreach { [pscustomobject]@{ AadCmdName = $_.AadCmdName; AadModuleName = $_.AadModuleName; GraphCmdName =''; GraphModuleName=''; GraphCmdScope=''; GraphUri=''; IsV1 = $false; IsBeta = $false} }) | Sort-Object -Property AadCmdName | Select-Object -Unique -Property AadCmdName, AadModuleName, GraphCmdName, GraphModuleName, GraphCmdScope, GraphUri, IsV1, IsBeta

#Load cmd into hash table
$allCmdMapHash = @{}
$allCmdMap | foreach{ $allCmdMapHash[$_.AadCmdName.ToLower()] = $_ }


#Read current mapping from docs
$docCmdMapCsv = Import-Csv .\DocCommandMap.csv
$docCmdMapHash = @{}
$docCmdMapCsv | foreach{
	if($_.GraphCmdName -eq $null) { $_.GraphCmdName = ''}
	$_.AadCmdName = $_.AadCmdName.Trim(); $_.GraphCmdName = $_.GraphCmdName.Trim(); $docCmdMapHash[$_.AadCmdName.ToLower()] = $_ 
}

#Add map info to new csv
$allCmdMapHash.GetEnumerator() | foreach{ if($docCmdMapHash.ContainsKey($_.Name)) { $_.Value.GraphCmdName = $docCmdMapHash[$_.Name].GraphCmdName } else { Write-Host "FYI" $_.Value.AadCmdName "is not mapped to a Graph command." } }

# Get the graph command and module names. Look in both v1 and beta since some commands are only available in v1 or beta
Select-MgProfile -Name beta 
UpdateGraphCommandModuleNameAndScope

Select-MgProfile -Name v1.0
UpdateGraphCommandModuleNameAndScope

$allCmdMapHash.GetEnumerator() | foreach{ if($_.Value.GraphCmdName -ne '' -and !($_.Value.IsV1 -or $_.Value.IsBeta)) { Write-Host $_.Value.GraphCmdName "was not found in Graph PowerShell module"  } }

# #Populate Graph scope
# $allCmdMapHash.GetEnumerator() | foreach{ $graphCmdName = $_.Value.GraphCmdName; if($graphCmdName -ne ''){ $graphCmd = Find-MgGraphCommand -Command $graphCmdName -ErrorAction SilentlyContinue; if ( $graphCmd -eq $null ) {Write-Host $graphCmdName "was not found in Graph PowerShell module"} else {$_.Value.GraphCmdScope=($graphCmd[0].Permissions | select -ExpandProperty Name) -join ";"; $_.Value.GraphUri=($graphCmd[0].Method + " " + $graphCmd[0].URI)} } }


#check for items in doc that are not in the PowerShell objects (most probably a typo in the doc that needs to be fixed)
$docCmdMapHash.GetEnumerator() | foreach{ if(-not $allCmdMapHash.ContainsKey($_.Value.AadCmdName.ToLower())) { Write-Host $_.Value.AadCmdName "in doc was not found in PowerShell module" } }

#Write .csv of command map to be used by converter
$allCmdMapHash.Values | Export-Csv .\CommandMap.csv -NoTypeInformation

#This command auto-maps the Graph Param to the AAD Param if the same name is found
#We can later test and manually override them.
#If Graph Param name is same as AAD Param then map them
$params | Where-Object {$_.AadParamName -ne ''} | foreach { 
	$key = $_.AadCmdName.ToLower(); 
	if ($allCmdMapHash.ContainsKey($key)) {
		$_.GraphCmdName = $allCmdMapHash[$key].GraphCmdName; 
		if ($_.GraphCmdName -ne ''){
			$graphCmdObj = (Get-Command $_.GraphCmdName -ErrorAction SilentlyContinue)
			if ($graphCmdObj -ne $null -and $graphCmdObj.Parameters.ContainsKey($_.AadParamName)) { 
				$_.GraphParamName =  $_.AadParamName
			}
		}
	}
}

$params | Where-Object {$_.AadParamName -ne ''} | Export-Csv .\ParamMap.csv -NoTypeInformation