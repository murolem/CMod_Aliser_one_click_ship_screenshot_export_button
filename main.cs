using Cosmoteer.Game;
using Halfling;
using Halfling.Application;
using Halfling.Geometry;
using Halfling.Gui;
using Halfling.Input;
using HarmonyLib;
using System.Runtime.CompilerServices;

[assembly: IgnoresAccessChecksTo("Cosmoteer")]
[assembly: IgnoresAccessChecksTo("HalflingCore")]

namespace CMod_Example {
    public class Main {
        public static Harmony? harmony;

        //Game and SimRoot contain all the information about the current game and simulation
        public static Cosmoteer.Game.GameRoot? gameRoot;
        public static Cosmoteer.Simulation.SimRoot? simRoot;
        public static Keyboard? keyboard;
        public static WeaponsToolbox? weaponsToolBox;
        public static LogBox? logBox;
        public static IAppState? lastFrameState;
        public static bool firstFrameStateSet = false;

        public static bool loaded;

        /// <summary>
        /// This function is called by the mod loader before the regular mods get loaded.
        /// It can be used to modify things such as .rules loading logic, so that the regular mods
        /// can utilize the new or modified functionality.
        /// </summary>
        /// <param name="cmods"></param>
        /// <param name="cmodIndex"></param>
        public static void BeforeAllRegularModsLoad(string[] cmods, int cmodIndex) {

        }

        /// <summary>
        /// This function is called by the mod loader after all of the regular mods get loaded.
        /// Use it to make major one-time patches and modifications, add listeners, etc.
        /// </summary>
        /// <param name="regularMods">A list of present regular mods IDs. CMods are not included here.</param>
        /// <param name="cmods">A list of present CMods.</param>
        /// <param name="cmodIndex">Index of this mod amongst other present CMods.</param>
        public static void AfterAllRegularModsLoaded(string[] regularMods, string[] cmods, int cmodIndex) {

            Harmony.DEBUG = true;
            FileLog.Log("initialize patches called");

            //Get Keyboard Object (Need this for key checking)
            keyboard = Halfling.App.Keyboard;

            //Subscribe to event which then gets called from the game thread
            Halfling.App.Director.FrameEnded += Worker;
        }

        /// <summary>
        /// This functions is called before the regular part of this mods gets loaded.
        /// </summary>
        /// <param name="regularMods">A list of present regular mods IDs. CMods are not included here.</param>
        /// <param name="cmods">A list of present CMods.</param>
        /// <param name="cmodIndex">Index of this mod amongst other present CMods.</param>
        public static void BeforeRegularModLoad(string[] regularMods, string[] cmods, int cmodIndex) {
            // your code here...
        }

        public static void AfterRegularModLoad() {
            // this functions is called after the regular part of this mods gets loaded
        }

        public static void Update() {
            // this functaion is called each frame that the application renders.
            // TODO: are there separate physics and regular frames?
        }

        public static void InitializePatches() {
            //This function gets called by the C++ Mod Loader and runs on the same thread as it
            //Only use this for initialization

            Harmony.DEBUG = true;
            FileLog.Log("initialize patches called");

            //Get Keyboard Object (Need this for key checking)
            keyboard = Halfling.App.Keyboard;

            //Subscribe to event which then gets called from the game thread
            Halfling.App.Director.FrameEnded += Worker;
        }


        public static void Worker(object? sender, EventArgs e) {
            //Will be called each Frame
            //UpdateGameStateTransitionChecker();

            //Gets current game state
            IAppState? currentState = App.Director.States.OfType<IAppState>().FirstOrDefault();
            if(!firstFrameStateSet) {
                lastFrameState = currentState;
                firstFrameStateSet = true;
                FileLog.Log("First app state: " + currentState.ToString());
            }

            if(currentState != lastFrameState) {
                FileLog.Log("App state change: " + currentState.ToString());
                lastFrameState = currentState;
            }


            //App.Director.UpdateStates

            if(currentState != null) {
                if(currentState.GetType() == typeof(GameRoot)) {
                    //We are ingame
                    // called once when loading the save for the first time
                    // new loading will not trigger this branch exection

                    if(!loaded) {
                        loaded = true;

                        Harmony.DEBUG = true;
                        harmony = new Harmony("com.company.project.product");
                        harmony.PatchAll();

                        FileLog.Log("harmony patch complete");

                        // your code







                        gameRoot = (GameRoot)currentState;
                        simRoot = gameRoot.Sim;

                        logBox = new LogBox(gameRoot);
                        logBox.LogInfo("beep boop");
                        //keyboard.CharTyped += Keyboard_CharTyped;

                        //Create Window

                        WeaponsToolbox weaponsToolBox = new WeaponsToolbox(gameRoot);
                        weaponsToolBox.SelfActive = false;
                        weaponsToolBox.Rect = new Rect(10f, 70f, 274f, 500f);
                        weaponsToolBox.ResizeController.MinSize = new Vector2(274f, 274f);

                        int labelButtonsCreatedCount = 0;
                        int labelButtonOffset = 20;
                        void LabelBtn(string text) {
                            Halfling.Gui.Label label = new();
                            //labelBtn1.PercentileWidth = 100
                            label.Text = text;
                            label.X = 0;
                            label.Y = labelButtonOffset * labelButtonsCreatedCount;
                            label.AutoSize.AutoWidthMode = Halfling.Gui.AutoSizeMode.Enable;
                            weaponsToolBox.AddChild(label);

                            labelButtonsCreatedCount++;
                        }

                        LabelBtn("hello crew meat");
                        LabelBtn("bye crew meat");

                        gameRoot.Gui.FloatingWindows.AddChild(weaponsToolBox);

                        Main.weaponsToolBox = weaponsToolBox;
                        weaponsToolBox.SelfActive = true; //Show Window
                    }
                }
            }

            //if(loaded)
            //{
            //    //Check if N Key was pressed
            //    bool result = keyboard.HotkeyPressed(ViKey.N, true);

            //    if(result)
            //    {
            //        //Key was pressed

            //        //Show / Hide Window
            //        Main.weaponsToolBox.SelfActive = !Main.weaponsToolBox.SelfActive;
            //    }
            //}
        }

        //private static void Keyboard_CharTyped(object? sender, CharTypedEventArgs e) {
        //    logBox.LogInfo("char typed: " + e.Char);
        //}

        enum GameStateTransition {
            TitleScreenEntered,
            LevelLoaded,
        }

        void UpdateGameStateTransitionChecker() {

        }
    }



    //[HarmonyPatch(typeof(Cosmoteer.Ships.Parts.Defenses.ArcShield))]
    //[HarmonyPatch("OnHit")] // if possible use nameof() here
    //class MyPatches {
    //    static void Postfix(Cosmoteer.Ships.Parts.Defenses.ArcShield __instance) {
    //        FileLog.Log("on shield hit!");
    //        __instance.Rules.Radius += (new Random().Next(-10, 10));
    //    }
    //}

    //A very basic Window class
    public class WeaponsToolbox : WindowBox {
        public WeaponsToolbox(GameRoot game) {
            //LayoutBox lbox = new LayoutBox();
            //lbox.Children.

            base.TitleText = "Test Window ";
            base.BoundsProvider = game.Gui.RootWidget;
            //base.Children.LayoutAlgorithm = LayoutAlgorithms.CenterLeft;
            base.Children.BorderPadding = new Borders(10f);
            base.Children.WidgetPadding = new Vector2(10f, 10f);
        }

    }
}