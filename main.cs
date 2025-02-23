using CMod;
using Cosmoteer;
using Cosmoteer.Game;
using Cosmoteer.Game.Gui;
using Cosmoteer.Gui;
using Cosmoteer.Ships;
using Halfling;
using Halfling.Application;
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
        /// Use this method to apply patches BEFORE the game is fully loaded 
        /// and BEFORE modifications to rules fules (actions, strings) are applied.
        /// 
        /// Source code reference: Cosmoteer.Mods.ModInfo.ApplyPreLoadMods()
        /// </summary>
        public static void Pre_ApplyPreLoadMods() {
            FileLogger.LogInfo("Pre_ApplyPreLoadMods() called");
        }

        /// <summary>
        /// Use this method to apply patches BEFORE the game is fully loaded 
        /// and AFTER modifications to rules fules (actions, strings) are applied.
        /// 
        /// Source code reference: Cosmoteer.Mods.ModInfo.ApplyPreLoadMods()
        /// </summary>
        public static void Post_ApplyPreLoadMods() {
            FileLogger.LogInfo("Post_ApplyPreLoadMods() called");
        }

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
        public static void Postfix(ref ShipsCard __instance, GameRoot game, GameGui gameGui, IRectProvider bounds) {
            // Game and SimRoot contain all the information about the current game and simulation
            //public static Cosmoteer.Game.GameRoot? gameRoot;
            //public static Cosmoteer.Simulation.SimRoot? simRoot;


            //Subscribe to event which then gets called from the game thread
            //Halfling.App.Director.FrameEnded += Update;

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

        private static void TakeScreenshotsButton_Clicked(object? sender, EventArgs e) {
            Ship? selectedShip = GameRoot.Current?.Sim?.PlayerInput?.GetSingleSelectedShip();
            if(selectedShip is null) {
                return;
            }

            Texture exteriorScreenshot = selectedShip.Renderer.CaptureFullSizeScreenShot(true);
            Texture interiorScreenshot = selectedShip.Renderer.CaptureFullSizeScreenShot(false);
            Texture blueprintScreenshot = selectedShip.BlueprintRenderer.CaptureFullSizeScreenShot();

            string shipName = selectedShip.Metadata.PlainShipName;

            SaveShipScreenshot(exteriorScreenshot, shipName, ScreenshotType.Exterior);
            SaveShipScreenshot(interiorScreenshot, shipName, ScreenshotType.Interior);
            SaveShipScreenshot(blueprintScreenshot, shipName, ScreenshotType.Blueprint);

            SaveUtils.ShowSavedDialog(
                Paths.ScreenshotsFolder,
                new string[] {
                    FormatShipFilename(shipName, ScreenshotType.Exterior),
                    FormatShipFilename(shipName, ScreenshotType.Interior),
                    FormatShipFilename(shipName, ScreenshotType.Blueprint),
                }
            );
        }

        enum ScreenshotType {
            Exterior,
            Interior,
            Blueprint
        }

        private static void SaveShipScreenshot(Texture screenshot, string shipName, ScreenshotType type) {
            SaveUtils.SaveAsImage(screenshot, new AbsolutePath(Paths.ScreenshotsFolder, FormatShipFilename(shipName, type)), ImageFileFormat.Png);
        }

        private static string FormatShipFilename(string shipName, ScreenshotType screenshotType) {
            string screenshotTypeSuffix;
            switch(screenshotType) {
                case ScreenshotType.Exterior:
                    screenshotTypeSuffix = "";
                    break;
                case ScreenshotType.Interior:
                    screenshotTypeSuffix = "Interior";
                    break;
                case ScreenshotType.Blueprint:
                    screenshotTypeSuffix = "Blueprint";
                    break;
                default:
                    throw new Exception("Unknown ship screenshot type: " + ScreenshotType.Blueprint);
            }

            return $"Ship{shipName}{screenshotTypeSuffix}.png";
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
                Util.ShellExecute((string)forDirpath);
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