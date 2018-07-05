$version = "v0.4-alpha"
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
    & $dotnet publish $project -c release -r $_ /p:TrimUnusedDependencies=true -o ("{0}\{1}" -f $output,$_)
}


$runtimes | %{
    $runtime = $_
    $toPack = ("{0}\{1}" -f $output,$runtime)
    $zipFile = ("{0}\{1}.{2}.{3}.zip" -f $output,$product,$version,$runtime)
    Compress-Archive -Path $toPack -DestinationPath $zipFile
}
