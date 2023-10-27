if ($args -eq $null -or $args[0] -eq "") {
   Write-Host "Please provide the path the dashboard settings json file, on your local file system."
   exit
}

Write-Host "Starting replace of dashboard settings file..."

$source = $args[0]
$dest = "/user/share/nginx/html/dashboardSettings.json"

# Get container Id by image name
# the Solution image name is livehealthchecks.ui
# the Docker Hub image name is shantanun/livehealthchecks.ui
$containerIdArray = docker ps -aqf "ancestor=shantanun/livehealthchecks.ui" --all

$containerId = $containerIdArray[0]

if ($containerId -eq $null -or $containerId -eq "") {
   Write-Host "Could not find a container by image name."
   exit
}

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