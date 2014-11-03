param($installPath, $toolsPath, $package, $project)

function Delete-Temporary-File 
{
	$project.ProjectItems | Where-Object { $_.Name -eq 'Shuttle.Core.MSBuildx.ReadMe.md' } | Foreach-Object {
		Remove-Item ( $_.FileNames(0) )
		$_.Remove() 
	}
}

function Get-SolutionDir {
    if($dte.Solution -and $dte.Solution.IsOpen) {
        return Split-Path $dte.Solution.Properties.Item("Path").Value
    }
    else {
        throw "Solution not available"
    }
}

function Copy-MSBuildTasks($project) {
	$solutionDir = Get-SolutionDir
	$tasksToolsPath = (Join-Path $solutionDir ".build")

	if(!(Test-Path $tasksToolsPath)) {
		mkdir $tasksToolsPath | Out-Null
	}

	Copy-Item "$toolsPath\Shuttle.Core.MSBuild.dll" $tasksToolsPath -Force | Out-Null
	Copy-Item "$toolsPath\Shuttle.Core.MSBuild.targets" $tasksToolsPath -Force | Out-Null

	$buildFile = Join-Path $solutionDir "package.msbuild"
	
	if(!(Test-Path $buildFile)) {
		Copy-Item "$toolsPath\package.msbuild" $solutionDir | Out-Null
	}

	return "$tasksToolsPath"
}

function Add-Solution-Folder($buildPath) {
	$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

	$buildFolder = $solution.Projects | Where {$_.ProjectName -eq ".build"}

	if (!$buildFolder) {
		$buildFolder = $solution.AddSolutionFolder(".build")
	}
	
	$projectItems = Get-Interface $buildFolder.ProjectItems ([EnvDTE.ProjectItems])

	$targetsPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "Shuttle.Core.MSBuild.targets") )
	$projectItems.AddFromFile($targetsPath)

	$dllPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "Shuttle.Core.MSBuild.dll") )
	$projectItems.AddFromFile($dllPath)

	$projPath = [IO.Path]::GetFullPath( (Join-Path $buildPath "..\package.msbuild") )
	$projectItems.AddFromFile($projPath)
}

# $taskPath = Copy-MSBuildTasks $project
# Add-Solution-Folder $taskPath

$scriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$assemblyPath = Join-Path $scriptPath "Shuttle.Core.MSBuild.dll"

[System.Reflection.Assembly]::LoadFrom($assemblyPath)

$Installation = New-Object -TypeName Shuttle.Core.MSBuild.Installation -ArgumentList $installPath, $toolsPath, $package, $project
$Installation.Execute()
