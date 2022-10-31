#set up pre submit git hook
$githookscript = @"
#!/bin/sh
exec dotnet test

if [ $? -ne 0 ]; then
echo "Tests must pass before committing!"
exit 1
fi
"@

if (Test-Path .\.git\hooks\pre-commit)
{
    Write-Warning "There already is a git pre-commit hook defined."
    Write-Warning "Please make sure you add the following to .\.git\hooks\pre-commit"
    Write-Output $githookscript
}
else
{
    New-Item -path .\.git\hooks -name pre-commit -type "file" -value $githookscript
    Write-Output "Created pre commit hook script"
}

#download and run seq docker container locally
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

#start local BLOB storage
docker run --name azurite -d --restart unless-stopped -p 10000:10000 -v {{path to your repo}}/infra/az-storage:/workspace mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0 --oauth basic --cert /workspace/certs/127.0.0.1.pem --key /workspace/certs/127.0.0.1-key.pem -l /workspace/files

# trust local https cert for dev
dotnet dev-certs https --trust

#restore solution
dotnet restore .\Incepted.sln

