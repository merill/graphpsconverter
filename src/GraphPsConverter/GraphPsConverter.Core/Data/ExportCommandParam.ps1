
#cd 'F:\Code\graphpsconverter\src\GraphPsConverter\GraphPsConverter.Core\Data\'

#Import-Module Microsoft.Graph #Takes time so load it once.

#utility methods to export all the parameters from aadPs and msol

$ignoreVariables = @("PipelineVariable","OutBuffer", "OutVariable", "InformationVariable", "InformationVariable", "WarningVariable", "ErrorInformationVariable", "Confirm", "WhatIf", "Verbose", "Debug", "ErrorVariable", "InformationAction", "WarningAction", "ErrorAction")


Remove-Module AzureADPreview
Import-Module AzureAD
Import-Module MSOnline
$params = Get-Command -Module MSOnline,azuread | foreach{ $cmd = $_.Name; [pscustomobject]@{AadCmdName=$cmd; GraphCmdName=''; AadParamName=''; GraphParamName=''}; if($_.Parameters -ne $null){ $_.Parameters.GetEnumerator() | foreach { if(-not $ignoreVariables.Contains($_.Value.Name)) {[pscustomobject]@{AadCmdName=$cmd; GraphCmdName=''; AadParamName=$_.Value.Name; GraphParamName=''} }} } }

Remove-Module azuread
Import-Module azureadpreview
$params += Get-Command -Module azureadpreview | foreach{ $cmd = $_.Name; [pscustomobject]@{AadCmdName=$cmd; GraphCmdName=''; AadParamName=''; GraphParamName=''}; if($_.Parameters -ne $null){ $_.Parameters.GetEnumerator() | foreach { if(-not $ignoreVariables.Contains($_.Value.Name)) {[pscustomobject]@{AadCmdName=$cmd;  GraphCmdName=''; AadParamName=$_.Value.Name; GraphParamName=''} }} } }

#Remove dups introduced by Azureadpreview
$params = $params | Sort-Object -Property AadCmdName,AadParamName | Select-Object -Unique -Property AadCmdName,GraphCmdName,AadParamName,GraphParamName

#Read all objects from Azure AD
$allCmdMap = ($params | foreach { [pscustomobject]@{ AadCmdName = $_.AadCmdName; GraphCmdName ='' } }) | Sort-Object -Property AadCmdName | Select-Object -Unique -Property AadCmdName, GraphCmdName

#Load into hash table
$allCmdMapHash = @{}
$allCmdMap | foreach{ $allCmdMapHash[$_.AadCmdName.ToLower()] = $_ }


#Read current mapping from docs
$docCmdMapCsv = Import-Csv .\DocCommandMap.csv
$docCmdMapHash = @{}
$docCmdMapCsv | foreach{ $docCmdMapHash[$_.AadCmdName.ToLower()] = $_ }

#Add map info to new csv
$allCmdMapHash.GetEnumerator() | foreach{ if($docCmdMapHash.ContainsKey($_.Name)) { $_.Value.GraphCmdName = $docCmdMapHash[$_.Name].GraphCmdName } else { Write-Host $_.Value.AadCmdName "was not found in doc" } }

#check for items in doc that are not in the PowerShell objects (most probably a typo in the doc that needs to be fixed)
$docCmdMapHash.GetEnumerator() | foreach{ if(-not $allCmdMapHash.ContainsKey($_.Value.AadCmdName.ToLower())) { Write-Host $_.Value.AadCmdName "in doc was not found in PowerShell module" } }

#Write .csv of command map to be used by converter
$allCmdMapHash.Values | Export-Csv .\CommandMap.csv -NoTypeInformation

#This command auto-maps the Graph PAram to the AAD Param if the same name is found
#We can later test and manually override them.
#If Graph Param name is same as AAD Param then map them
$params | Where-Object {$_.AadParamName -ne ''} | foreach { $key = $_.AadCmdName.ToLower(); if ($allCmdMapHash.ContainsKey($key)) {$_.GraphCmdName = $allCmdMapHash[$key].GraphCmdName; if (  $_.GraphCmdName -ne '' -and (Get-Command $allCmdMapHash[$key].GraphCmdName).Parameters.ContainsKey($_.AadParamName)) { $_.GraphParamName =  $_.AadParamName}  }}

$params | Where-Object {$_.AadParamName -ne ''} | Export-Csv .\ParamMap.csv -NoTypeInformation