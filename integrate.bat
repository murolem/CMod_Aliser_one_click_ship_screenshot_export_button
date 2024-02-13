set release_dir_path="C:\Users\aliser\Desktop\repos\CMod_Example\bin\Release\net7.0-windows"
set mod_dir_path="C:\Users\aliser\Saved Games\Cosmoteer\76561198068709671\Mods\cmod_example"

@REM stop cosmoteer process if its running
taskkill /f /im cosmoteer.exe /FI "STATUS eq RUNNING"

@REM sleep for a few seconds, so the process can end
ping 127.0.0.1 -n 3 > nul

@REM create bin dir if it doesn't exist
if not exist %mod_dir_path%"\bin\" mkdir %mod_dir_path%"\bin\"

@REM copy stuff in
copy %release_dir_path%"\main.dll" %mod_dir_path%"\bin\"
copy %release_dir_path%"\main.pdb" %mod_dir_path%"\bin\"
copy %release_dir_path%"\main.runtimeconfig.json" %mod_dir_path%"\bin\"

copy %release_dir_path%"\0Harmony.dll" %mod_dir_path%"\bin\"
copy %release_dir_path%"\Mono.Cecil.*" %mod_dir_path%"\bin\"
copy %release_dir_path%"\MonoMod.Common.*" %mod_dir_path%"\bin\"


@REM restart cosmoteer 
start "" "C:\Program Files (x86)\Steam\steamapps\common\Cosmoteer\Bin\Cosmoteer.exe" --devmode