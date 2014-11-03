
'Set-MSBuildProperty', 'Add-SolutionDirProperty', 'Add-Import', 'Remove-Import', 'Add-SolutionDirProperty' | %{ 
    Register-TabExpansion $_ @{
        ProjectName = { Get-Project -All | Select -ExpandProperty Name }
    }
}

Register-TabExpansion 'Get-MSBuildProperty' @{
    ProjectName = { Get-Project -All | Select -ExpandProperty Name }
    PropertyName = {param($context)
        if($context.ProjectName) {
            $buildProject = Get-MSBuildProject $context.ProjectName
        }
        
        if(!$buildProject) {
            $buildProject = Get-MSBuildProject
        }
        
        $buildProject.Xml.Properties | Sort Name | Select -ExpandProperty Name -Unique
    }
}

Export-ModuleMember Get-MSBuildProject, Add-SolutionDirProperty, Add-Import, Remove-Import, Get-MSBuildProperty, Set-MSBuildProperty, Get-SolutionDir