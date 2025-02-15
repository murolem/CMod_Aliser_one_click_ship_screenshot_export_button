set release_dir_path="bin\Release\net9.0-windows7.0"
set mods_dir_path="C:\Program Files (x86)\Steam\steamapps\common\Cosmoteer\Bin\CMods"
set mod_dir="Example"

set mod_dir_path=%mods_dir_path%"\"%mod_dir%

@REM stop cosmoteer process if its running
taskkill /f /im cosmoteer.exe /FI "STATUS eq RUNNING"

@REM sleep for a few seconds, so the process can end
ping 127.0.0.1 -n 3 > nul

@REM create the mods dir if not exists
if not exist %mods_dir_path% mkdir %mods_dir_path%

@REM create the mod dir if not exists
if not exist %mod_dir_path% mkdir %mod_dir_path%

@REM copy stuff in
cd %release_dir_path%

copy "Main.dll" %mod_dir_path%
copy "Main.pdb" %mod_dir_path%
copy "Main.runtimeconfig.json" %mod_dir_path%

copy "0Harmony.dll" %mod_dir_path%
copy "Mono.Cecil.*" %mod_dir_path%
copy "MonoMod.Common.*" %mod_dir_path%


@REM restart cosmoteer 
start "" "C:\Program Files (x86)\Steam\steamapps\common\Cosmoteer\Bin\Cosmoteer.exe" --devmode