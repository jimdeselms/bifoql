# This is a Powershell script, and it should probably be bashified if we want to
# create a jenkins job to do the same thing.
Param ($buildNumber, [switch]$noPush)

if (!$buildNumber)
{
	$buildNumber = (& "$($psscriptroot)\Get-HighestVersionNumber") + 1

	"Using next available build number $buildNumber"
}

$push = !$noPush
$buildPath = Join-Path $psscriptroot ..

# Build docker image from pwd with BUILD_NUMBER

docker build `
	-t bifoql:$($buildNumber)-staging $buildPath

# Tag the current image with specific BUILD_NUMBER version

docker tag bifoql:$($buildNumber)-staging 779051441487.dkr.ecr.eu-west-1.amazonaws.com/bifoql:$($buildNumber)-staging

"Built version $buildNumber."

if ($push)
{
	# Login to EC2 Repo via Docker
	aws ecr get-login --no-include-email --region eu-west-1 | iex

	# Create the repository if it doesn't already exist.
	$repos = aws ecr describe-repositories --query "repositories[].repositoryName" | Out-String | ConvertFrom-Json
	if ($repos -notcontains "bifoql")
	{
		"Creating ECR repository bifoql"
		aws ecr create-repository --repository-name bifoql
	}

	# Push the current version

	docker push 779051441487.dkr.ecr.eu-west-1.amazonaws.com/bifoql:$($buildNumber)-staging
}
"Pushed version $buildNumber to the ECR."
"To deploy to the staging environment, run Deploy-Staging $buildNumber"
