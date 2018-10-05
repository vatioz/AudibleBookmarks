$version = "v0.8-alpha"
$product = "AudibleBookmarks"
$project = "D:\_sources\AudibleBookmarks\AvaloniaUI\AvaloniaUI.csproj"
$output = "D:\_sources\AudibleBookmarks\_releases"
$dotnet = "C:\Program Files\dotnet\dotnet.exe"
$runtimes = @(
"win10-x64"
"win10-x86"
"win-x64"
"win-x86"
"osx-x64"
"linux-x64"
)

Remove-Item "$output\*" -Recurse -Confirm:$true

$runtimes | %{
    & $dotnet publish $project -c release -r $_  -o ("{0}\{1}" -f $output,$_)
}

$assets = @()

$runtimes | %{
    $runtime = $_
    $toPack = ("{0}\{1}" -f $output,$runtime)
    $fileName = "{0}.{1}.{2}" -f $product,$version,$runtime
    $zipFile = ("{0}\{1}.zip" -f $output,$fileName)
    Compress-Archive -Path $toPack -DestinationPath $zipFile
    $asset = @{
        Path=$zipFile;
        "Content-Type"="application/zip";
        Name = $fileName
    }
    $assets += $asset
}



$tmp = Set-GitHubSessionInformation -Username vatioz
$release = New-GitHubRelease -Repository $product -Name "$product $version" -Tag $version -Asset $assets -Prerelease -Verbose
Write-Information "Path to release: $($release.html_url)"
# show me
Start-Process $release.html_url

