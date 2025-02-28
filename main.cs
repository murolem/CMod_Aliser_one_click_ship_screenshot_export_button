using CMod;
using Cosmoteer;
using Cosmoteer.Game;
using Cosmoteer.Game.Gui;
using Cosmoteer.Gui;
using Cosmoteer.Ships;
using Halfling;
using Halfling.Application;
using Halfling.Collections;
using Halfling.Graphics;
using Halfling.Gui;
using Halfling.Gui.Components.Rects;
using Halfling.Gui.Dialogs;
using Halfling.IO;
using Halfling.Logging;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;

//[assembly: AssemblyVersion("1.1.0")]
[assembly: IgnoresAccessChecksTo("Cosmoteer")]
[assembly: IgnoresAccessChecksTo("HalflingCore")]

namespace CModEntrypoint_Aliser_one_click_ship_screenshot_export_button {
    public class Main {
        public static Harmony? harmony;

        /// <summary>
        /// Use this method to apply patches AFTER the game is fully loaded 
        /// and BEFORE modifications are done to the loaded game data (ship libraries being added).
        /// 
        /// Source code reference: Cosmoteer.Mods.ModInfo.ApplyPostLoadMods()
        /// </summary>
        public static void Pre_ApplyPostLoadMods() {
            FileLogger.LogInfo("Pre_ApplyPostLoadMods() called");
        }

        /// <summary>
        /// Use this method to apply patches AFTER the game is fully loaded 
        /// and AFTER modifications are done to the loaded game data (ship libraries being added).
        /// 
        /// Source code reference: Cosmoteer.Mods.ModInfo.ApplyPostLoadMods()
        /// </summary>
        public static void Post_ApplyPostLoadMods() {
            FileLogger.LogInfo("Post_ApplyPostLoadMods() called");

            harmony = new Harmony("cmod.aliser.one_click_ship_screenshot_export_button");

            // enable if you want ot have harmony logging about your patching.
            // if enabled, will create a log file on your systems Desktop called "harmony.log.txt".
            Harmony.DEBUG = false;

            FileLog.Log("Running patches");
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);

            ShipsCardPatch.AfterPatch();
        }
    }


    [HarmonyPatch(typeof(ShipsCard), MethodType.Constructor)] // Class name
    [HarmonyPatch("ShipsCard")] // Constructor name
    [HarmonyPatch(new Type[]
        {
        typeof(GameRoot), typeof(GameGui), typeof(IRectProvider)
        })
    ] // Constructor argument types

    static class ShipsCardPatch {
        public static string[] builtInShipsPaths = [];
        public static void AfterPatch() {
            // scan built in ships
            builtInShipsPaths = ScanForShipFilesInDirectoryRecursive(ShipLibrary.BuiltIn.Folder)
                .ToArray();
        }

        public static void Postfix(ref ShipsCard __instance, GameRoot game, GameGui gameGui, IRectProvider bounds) {
            var modDirPath = Utils.GetPathToModRoot();

            var container = (LayoutBox)__instance.DockedChildren[0];
            var shipInteriorButton = container.Children.Last();

            AbsolutePath absolutePath = (AbsolutePath)Path.Combine(modDirPath, "assets", "button-icon.png");
            TextureFactory textures = App.Graphics.Textures;

            string filepath = (string)(FilePath)absolutePath;
            int mipLevels = 2;
            TextureSampleMode? sampleMode = TextureSampleMode.Point;
            Texture texture = textures.Load(filepath, mipLevels: mipLevels, sampleMode: sampleMode);

            var takeScreenshotsButton = new ImageButton(texture);
            takeScreenshotsButton.CopySettingsFrom(WidgetRules.Instance.FlatButton);
            takeScreenshotsButton.Size = shipInteriorButton.Size;
            takeScreenshotsButton.ToolTips.Text = "Capture screenshots of exterior, interior and blueprint views, saving them to the screenshots folder.";

            takeScreenshotsButton.Clicked += TakeScreenshotsButton_Clicked;

            container.AddChild((Widget)takeScreenshotsButton);
        }

        /// <summary>
        /// Scans directory and it's subdirectories searching for ship files.
        /// 
        /// Returns a list of paths to all found ship files.
        /// </summary>
        /// <param name="rootDirPath"></param>
        /// <returns></returns>
        public static IEnumerable<string> ScanForShipFilesInDirectoryRecursive(string rootDirPath) {
            string[] dirPaths = Directory.GetDirectories(rootDirPath);
            if(dirPaths.Length > 0) {
                // more dirs
                return dirPaths
                        .SelectMany(ScanForShipFilesInDirectoryRecursive);
            } else {
                // ship files
                return Directory.GetFiles(rootDirPath, "*.ship.png");
            }
        }

        private static void TakeScreenshotsButton_Clicked(object? sender, EventArgs e) {
            FileLogger.LogInfo("Screenshot button clicked, processing.");

            Ship? selectedShip = GameRoot.Current?.Sim?.PlayerInput?.GetSingleSelectedShip();
            if(selectedShip == null) {
                FileLogger.LogInfo("Selected ship is null, aborting.");
                return;
            }

            string shipName = selectedShip.Metadata.PlainShipName;

            FileLogger.LogInfo("Currently selected ship name: " + shipName);

            FileLogger.LogInfo("Searching for the ship in built-ins");

            string? builtInShipPath = TryFindBuiltInShip(shipName);
            string? builtInShipRelativePath = null;
            if(builtInShipPath == null) {
                FileLogger.LogInfo("Ship was not found in built-ins. Using just the ship's name for its filename.");
            } else {
                FileLogger.LogInfo("Found the ship in built-ins! Its filename will be modified to include its location in built-ins.");

                // relative to built-ins
                builtInShipRelativePath = Path.GetRelativePath(ShipLibrary.BuiltIn.Folder, builtInShipPath);

                FileLogger.LogInfo("Location relative to built-ins: " + builtInShipRelativePath);
            }

            FileLogger.LogInfo("Generating screenshots");

            Texture exteriorScreenshot = selectedShip.Renderer.CaptureFullSizeScreenShot(true);
            Texture interiorScreenshot = selectedShip.Renderer.CaptureFullSizeScreenShot(false);
            Texture blueprintScreenshot = selectedShip.BlueprintRenderer.CaptureFullSizeScreenShot();

            FileLogger.LogInfo("Saving screenshots");

            SaveShipScreenshot(exteriorScreenshot, ScreenshotType.Exterior, shipName, builtInShipRelativePath ?? "");
            SaveShipScreenshot(interiorScreenshot, ScreenshotType.Interior, shipName, builtInShipRelativePath ?? "");
            SaveShipScreenshot(blueprintScreenshot, ScreenshotType.Blueprint, shipName, builtInShipRelativePath ?? "");

            FileLogger.LogInfo("Showing post-save dialog");

            SaveUtils.ShowSavedDialog(
                Paths.ScreenshotsFolder,
                new string[] {
                    ConstructScreenshotFilename(shipName, ScreenshotType.Exterior, builtInShipRelativePath ?? ""),
                    ConstructScreenshotFilename(shipName, ScreenshotType.Interior, builtInShipRelativePath ?? ""),
                    ConstructScreenshotFilename(shipName, ScreenshotType.Blueprint, builtInShipRelativePath ?? ""),
                }
            );

            FileLogger.Separator();
        }

        enum ScreenshotType {
            Exterior,
            Interior,
            Blueprint
        }

        private static void SaveShipScreenshot(Texture screenshot, ScreenshotType type, string shipName, string shipLocation) {
            string resultingFilename = ConstructScreenshotFilename(shipName, type, shipLocation);

            FileLogger.LogInfo("Resulting filename: " + resultingFilename);

            AbsolutePath resultingPath = new AbsolutePath(Paths.ScreenshotsFolder, resultingFilename);

            FileLogger.LogInfo("Saving to: " + resultingPath);

            SaveUtils.SaveAsImage(screenshot, resultingPath, ImageFileFormat.Png);
        }

        /// <summary>
        /// Constructs a filename for a ship screenshot.
        /// </summary>
        /// <param name="shipName">Ship name.</param>
        /// <param name="screenshotType">Screenshot type.</param>
        /// <param name="shipLocation">Path to the ship file relative to some ship library directory.</param>
        /// <returns></returns>
        private static string ConstructScreenshotFilename(string shipName, ScreenshotType screenshotType, string shipLocation) {
            string shipNameFormatted = FormatShipName(shipName);
            string screenshotTyepFormatted = FormatScreenshotType(screenshotType);
            string shipLocationFormatted = FormatShipLocation(shipLocation);

            return $"Ship{shipLocationFormatted}{shipNameFormatted}{screenshotTyepFormatted}.png";
        }

        /// <summary>
        /// Formats ship relative location to some ship library directory to use in a screenshot filename.
        /// </summary>
        /// <param name="shipLocation">Relative location to some ship library directory.</param>
        /// <returns></returns>
        private static string FormatShipLocation(string shipLocation) {
            // get dir path and remove spaces
            string formatted = String.Join(
                "",
                shipLocation
                    .Split(Path.DirectorySeparatorChar)
                    [0..^1]
                );


            // replace some word occurances
            formatted = formatted.Replace("Stations", "Station");

            return formatted;
        }

        /// <summary>
        /// Formats ship name to use in screenshot filename.
        /// </summary>
        /// <returns></returns>
        private static string FormatShipName(string shipName) {
            // remove all spaces
            shipName = shipName.Replace(" ", "");

            return shipName;
        }

        /// <summary>
        /// Formats screenshot type to use in screenshot filename.
        /// </summary>
        /// <param name="shipName"></param>
        /// <returns></returns>
        private static string FormatScreenshotType(ScreenshotType screenshotType) {
            switch(screenshotType) {
                case ScreenshotType.Exterior:
                    return "";
                case ScreenshotType.Interior:
                    return "Interior";
                case ScreenshotType.Blueprint:
                    return "Blueprint";
                default:
                    throw new Exception("Unknown ship screenshot type: " + screenshotType);
            }
        }

        /// <summary>
        /// Searches for a ship by name in the built-ins ship library.
        /// If found, return the absolute path to the found ship file.
        /// </summary>
        /// <param name="shipLibrary"></param>
        /// <param name="shipName"></param>
        /// <returns></returns>
        static string? TryFindBuiltInShip(string shipName) {
            if(builtInShipsPaths.Length == 0) {
                FileLogger.LogError("No scanned built-in ships found. Forgot to run a scan, dear dev?");
                return null;
            }

            string? foundShipFilePath = builtInShipsPaths.Find(shipFilePath => {
                //FileLogger.LogDebug("Checking: " + shipFilePath);
                string? foundShipName = Ship.GetNameFromFilename(shipFilePath);
                if(foundShipName == null) {
                    return false;
                }

                if(foundShipName == shipName) {
                    //FileLogger.LogDebug("Match!");
                    return true;
                }

                return false;
            });

            if(foundShipFilePath == null) {
                return null;
            }

            return foundShipFilePath;
        }

    }

    class SaveUtils {
        /// <summary>
        /// Shows a dialog telling the user where a screenshot was saved and providing an option of opening that folder.
        /// </summary>
        public static async void ShowSavedDialog(AbsolutePath forDirpath, string[] savedFilenames) {
            List<string> messageParts = new();
            messageParts.Add("Saved to files: <b>");
            foreach(var filename in savedFilenames) {
                messageParts.Add(StringTools.EscapeForXml($"• {filename}"));
            }
            messageParts.Add("</b>");
            messageParts.Add("Screenshots directory:");
            messageParts.Add($"<b>{StringTools.EscapeForXml(forDirpath)}</b>");

            string message = String.Join("\n", messageParts);

            TwoButtonDialog dialog = new TwoButtonDialog(message, option2Text: "Take me there");
            dialog.MessageLabel.TextRenderer.HAlignment = Halfling.Graphics.Text.HAlignment.Left;

            await App.Director.PushState((IAppState)dialog);
            TwoButtonDialog.DialogResponse response = dialog.Response;
            dialog = (TwoButtonDialog)null;

            if(response != TwoButtonDialog.DialogResponse.Option2) {
                return;
            }

            try {
                Halfling.Util.ShellExecute((string)forDirpath);
            } catch(Exception ex) {
                OneButtonDialog.Show(ex.Message);
            }
        }

        /// <summary>Saves the frame as an image file.</summary>
        public static void SaveAsImage(Texture frame, AbsolutePath filepath, ImageFileFormat format) {
            try {
                Directory.CreateDirectory((string)filepath.Directory);
                using(Stream stream = (Stream)File.OpenWrite((string)(FilePath)filepath)) {
                    SaveImageTo(stream, frame, format);
                }
            } catch(Exception ex) {
                Logger.Log(ex.ToString());
                OneButtonDialog.Show(StringTools.EscapeForXml(ex.Message));
            }
        }

        /// <summary>Saves the frame as an image to the specified stream.</summary>
        public static void SaveImageTo(Stream stream, Texture frame, ImageFileFormat format) {
            using(TextureData data = _GetData()) {
                data.SaveTo(stream, format);
            }

            TextureData _GetData() {
                using(App.Graphics.BeginDraw())
                    return frame.ToData();
            }
        }
    }
}