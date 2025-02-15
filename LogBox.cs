using Cosmoteer.Game;
using Halfling.Geometry;
using Halfling.Gui;
using HarmonyLib;

namespace CMod_Example {
    /// <summary>
    /// An in-game logging utility, that displays a window with all messages logged.
    /// 
    /// Also saves each message to harmony output logfile.
    /// </summary>
    public class LogBox {
        private readonly ScrollBox logBoxContainer;

        private readonly char[] _visbilitySwitchHotkeys = new char[] {
            '`',
            'ё',
            'Ё'
        };

        public LogBox(
            GameRoot gameRoot,
            float percentileHeight = 40,
            float marginTop = 10,
            float marginLeft = 10,
            float marginRight = 10,
            float logLinesGap = 4
        ) {
            var guiRoot = gameRoot.Gui.RootWidget;

            this.logBoxContainer = new ScrollBox() {
                AnchorRect = AnchorPresets.TopStretch,
                PercentileHeight = percentileHeight,
                SelfActive = false,
            };

            this.logBoxContainer.Top = marginTop;
            this.logBoxContainer.Left = marginLeft;
            this.logBoxContainer.Right = -marginRight;

            // widget gap
            this.logBoxContainer.Children.WidgetPadding = new Vector2(
                Halfling.Gui.Label.Default.TextRenderer.FontSize // makes child widgets not overlap
                + logLinesGap // additinal pixel gap
            );
            // container padding
            this.logBoxContainer.Children.BorderPadding = new Borders(10f);

            this.logBoxContainer.Children.LayoutAlgorithm = LayoutAlgorithms.TileBottomLeftVertical;


            // adding to the gui root root
            guiRoot.AddChild(this.logBoxContainer);
            guiRoot.SelfDeactivated += this.GuiRoot_SelfDeactivated;
            guiRoot.RenderingDeactivated += this.GuiRoot_RenderingDeactivated;
            guiRoot.Deactivated += this.GuiRoot_Deactivated;
            guiRoot.SelfRenderingDeactivated += this.GuiRoot_SelfRenderingDeactivated;

            // listen for open key
            var keyboard = Halfling.App.Keyboard;
            //keyboard.CharTyped += this.CharTyped;

            this.LogWarn("warn message");
            this.LogError("error message");
        }

        private void GuiRoot_SelfRenderingDeactivated(object? sender, EventArgs e) {
            this.LogInfo("root gui self rendering deactivated");
        }

        private void GuiRoot_Deactivated(object? sender, EventArgs e) {
            this.LogInfo("root gui deactivated");
        }

        private void GuiRoot_RenderingDeactivated(object? sender, EventArgs e) {
            this.LogInfo("root gui rendering self deactivated");
        }

        private void GuiRoot_SelfDeactivated(object? sender, EventArgs e) {
            this.LogInfo("root gui self deactivated");
        }

        //private void CharTyped(object? sender, CharTypedEventArgs e) {
        //    if(_visbilitySwitchHotkeys.Contains(e.Char)) {

        //        // show or hide the log box
        //        this.logBoxContainer.SelfActive = !this.logBoxContainer.SelfActive;
        //    }
        //}

        public void LogInfo(string message) {
            this.AppendMessage("info: " + message);
        }

        public void LogWarn(string message) {
            var label = this.AppendMessage("warn: " + message);
            label.TextRenderer.Color = Halfling.Graphics.Color.Black;
            label.TextRenderer.BackgroundColor = Halfling.Graphics.Color.Yellow;
        }

        public void LogError(string message) {
            var label = this.AppendMessage("error: " + message);
            label.TextRenderer.Color = Halfling.Graphics.Color.White;
            label.TextRenderer.BackgroundColor = Halfling.Graphics.Color.Red;
        }

        private Halfling.Gui.Label AppendMessage(string message) {
            // append a label to the gui container
            var label = new Halfling.Gui.Label() {
                Text = message
            };

            label.AutoSize.AutoWidthMode = Halfling.Gui.AutoSizeMode.Enable;

            // apend at start so the new messages appear at lower part of the container
            this.logBoxContainer.Children.Insert(0, label);

            // append the message to the log file
            FileLog.Log("LOGBOX: " + message);

            return label;
        }
    }
}
