if ($args -eq $null -or $args[0] -eq "") {
   Write-Host "Please provide the path the dashboard settings json file, on your local file system."
   exit
}

Write-Host "Starting replace of dashboard settings file..."

$source = $args[0]
$dest = "/user/share/nginx/html/dashboardSettings.json"

# Get container Id by image name
$containerId = docker ps -aqf "ancestor=livehealthchecks.ui" --all

Write-Host "The container Id is $($containerId)..."

$docker_arguments = @("stop",                                               `
                  "$($containerId)")

Write-Host "Stopping container..."
docker $docker_arguments

Write-Host "Replacing dashboard settings file..."

$docker_arguments = @("cp",
		  "$($source)",                                               `
                  "$($containerId):$($dest)")

docker $docker_arguments

Write-Host "Starting container..."
$docker_arguments = @("start",                                               `
                  "$($containerId)")

docker $docker_arguments

Write-Host "Dashboard settings file replaced successfully."