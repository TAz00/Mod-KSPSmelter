

$fromPath = "C:\Users\Daniel\Documents\visual studio 2013\Projects\KSPSmelter\KSPSmelter\bin\Debug\"
$basePath = "D:\KSPDev\Kerbal Space Program\"
$toPath = "GameData\KSPSmelter\Plugins\"
try
{
Remove-Item -Path ($basepath + $toPath + "KSPSmelter.dll") -Force
Remove-Item -Path ($basepath + $toPath + "KSPSmelter.pdb") -Force
Remove-Item -Path ($basepath + $toPath + "KSPSmelter.dll.mdb") -Force

sleep -Milliseconds 500

Copy-Item -Path ($fromPath + "KSPSmelter.dll") -Destination ($basepath + $toPath + "KSPSmelter.dll") -Force
Copy-Item -Path ($fromPath + "KSPSmelter.pdb") -Destination ($basepath + $toPath + "KSPSmelter.pdb") -Force

sleep -Milliseconds 500
cd ($basepath + $toPath)
Start-Process -FilePath ($basepath + $toPath + "CreateMDB.bat") -WorkingDirectory ($basepath + $toPath) -Wait

sleep -Milliseconds 500


Start-Process -FilePath ($basePath + "KSP.exe") -WorkingDirectory $basePath

exit 0
}
catch
{
exit 1
}
