param($installPath, $toolsPath, $package, $project)

"Installing [{0}] to project [{1}]" -f $package.Id, $project.FullName | Write-Host

# The native PhysX DLLs (x64) are needed
# They are set to 'Copy if newer', so when the project is built, they will copied out to your bin\xyz directory
$nativePhysXDllsX64 = "nvToolsExt32_1.dll", 
	"PhysX3DEBUG_x86.dll",
	"PhysX3CommonDEBUG_x86.dll",
	"PhysX3CookingDEBUG_x86.dll",
	"PxFoundationDEBUG_x86.dll"

ForEach ($dll in $nativePhysXDllsX64)
{
	# Set 'Copy To Output Directory' to 'Copy if newer'
		# 0 = Do not copy
		# 1 = Copy always
		# 2 = Copy if newer

	$handle = $project.ProjectItems.Item($dll)

	$copyToOutput = $handle.Properties.Item("CopyToOutputDirectory")
	$copyToOutput.Value = 2
}

# Save the project
$project.Save()