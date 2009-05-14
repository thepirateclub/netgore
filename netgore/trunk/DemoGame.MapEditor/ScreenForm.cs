using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using DemoGame.Client;
using DemoGame.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NetGore;
using NetGore.EditorTools;
using NetGore.Graphics;
using Color=System.Drawing.Color;
using Point=System.Drawing.Point;

// LATER: Grid-snapping for batch movement
// LATER: When walking down slope, don't count it as falling
// LATER: Add more support for editing Grhs
// LATER: Add a cursor that can work with misc entities

namespace DemoGame.MapEditor
{
    partial class ScreenForm : Form, IGetTime
    {
        /// <summary>
        /// Key to move the camera down
        /// </summary>
        const Keys _cameraDown = Keys.S;

        /// <summary>
        /// Key to move the camera left
        /// </summary>
        const Keys _cameraLeft = Keys.A;

        /// <summary>
        /// Rate at which the screen scrolls
        /// </summary>
        const float _cameraMoveRate = 15;

        /// <summary>
        /// Key to move the camera right
        /// </summary>
        const Keys _cameraRight = Keys.D;

        /// <summary>
        /// Key to move the camera up
        /// </summary>
        const Keys _cameraUp = Keys.W;

        static readonly Color _colorChanged = Color.Lime;
        static readonly Color _colorError = Color.Red;
        static readonly Color _colorNormal = SystemColors.Window;

        /// <summary>
        /// Color of the Grh preview when placing new Grhs
        /// </summary>
        static readonly Microsoft.Xna.Framework.Graphics.Color _drawPreviewColor = new Microsoft.Xna.Framework.Graphics.Color(
            255, 255, 255, 150);

        readonly AddGrhCursor _addGrhCursor = new AddGrhCursor();
        readonly AddWallCursor _addWallCursor = new AddWallCursor();

        /// <summary>
        /// Screen camera
        /// </summary>
        readonly Camera2D _camera = new Camera2D(GameData.ScreenSize);

        /// <summary>
        /// Camera used on the Grh edit display
        /// </summary>
        readonly Camera2D _editGrhCamera = new Camera2D(GameData.ScreenSize);

        readonly EntityCursor _entityCursor = new EntityCursor();
        readonly GrhCursor _grhCursor = new GrhCursor();

        /// <summary>
        /// Draws the grid
        /// </summary>
        readonly ScreenGrid _grid = new ScreenGrid(GameData.ScreenSize);

        readonly MapBorderDrawer _mapBorderDrawer = new MapBorderDrawer();

        /// <summary>
        /// Information on the walls bound to MapGrhs
        /// </summary>
        readonly MapGrhWalls _mapGrhWalls;

        /// <summary>
        /// Currently selected Grh to draw to the map
        /// </summary>
        readonly Grh _selectedGrh = new Grh(null, AnimType.Loop, 0);

        /// <summary>
        /// Stopwatch used for calculating the game time (cant use XNA's GameTime since the
        /// form does not inherit DrawableGameComponent)
        /// </summary>
        readonly Stopwatch _stopWatch = new Stopwatch();

        /// <summary>
        /// List of all the active transformation boxes
        /// </summary>
        readonly List<TransBox> _transBoxes = new List<TransBox>(9);

        readonly WallCursor _wallCursor = new WallCursor();

        /// <summary>
        /// Current world - used for reference by the map being edited only
        /// </summary>
        readonly World _world;

        /// <summary>
        /// All content used by the map editor
        /// </summary>
        ContentManager _content;

        /// <summary>
        /// Current total time in milliseconds - used as the root of all timing
        /// in external classes through the GetTime method
        /// </summary>
        int _currentTime = 0;

        /// <summary>
        /// World position of the cursor
        /// </summary>
        Vector2 _cursorPos = Vector2.Zero;

        /// <summary>
        /// Grh display of the _editGrhData
        /// </summary>
        Grh _editGrh = null;

        /// <summary>
        /// GrhData currently being edited with the EditGrhForm
        /// </summary>
        GrhData _editGrhData = null;

        /// <summary>
        /// ListBox control object collection containing the walls for the Grh being edited
        /// </summary>
        ListBox.ObjectCollection _editGrhWallItems = null;

        /// <summary>
        /// TreeNode Grh currently being edited with the EditGrhForm
        /// </summary>
        TreeNode _editNode = null;

        /// <summary>
        /// Default font
        /// </summary>
        SpriteFont _font;

        KeyEventArgs _keyEventArgs = new KeyEventArgs(Keys.None);

        /// <summary>
        /// Current map
        /// </summary>
        Map _map;

        /// <summary>
        /// Currently pressed mouse button
        /// </summary>
        MouseButtons _mouseButton = MouseButtons.None;

        /// <summary>
        /// Modifier values for the camera moving
        /// </summary>
        Vector2 _moveCamera;

        /// <summary>
        /// Global SpriteBatch used by the editor
        /// </summary>
        SpriteBatch _sb;

        /// <summary>
        /// Currently selected cursor from the cursor toolbar
        /// </summary>
        EditorCursorBase _selectedTool = null;

        /// <summary>
        /// Currently selected transformation box
        /// </summary>
        TransBox _selTransBox = null;

        /// <summary>
        /// Wall currently being edited
        /// </summary>
        WallEntity _wallToEdit = null;

        public AddGrhCursor AddGrhCursor
        {
            get { return _addGrhCursor; }
        }

        public AddWallCursor AddWallCursor
        {
            get { return _addWallCursor; }
        }

        /// <summary>
        /// Gets the camera used for the game screen
        /// </summary>
        public Camera2D Camera
        {
            get { return _camera; }
        }

        /// <summary>
        /// Gets or sets the cursor position, taking the camera position into consideration
        /// </summary>
        public Vector2 CursorPos
        {
            get { return _cursorPos; }
            set { _cursorPos = value; }
        }

        public EntityCursor EntityCursor
        {
            get { return _entityCursor; }
        }

        public GameScreenControl GameScreenControl
        {
            get { return GameScreen; }
        }

        public GrhCursor GrhCursor
        {
            get { return _grhCursor; }
        }

        /// <summary>
        /// Gets the grid used for the game screen
        /// </summary>
        public ScreenGrid Grid
        {
            get { return _grid; }
        }

        /// <summary>
        /// Gets the most recent KeyEventArgs
        /// </summary>
        public KeyEventArgs KeyEventArgs
        {
            get { return _keyEventArgs; }
        }

        /// <summary>
        /// Gets the currently loaded map
        /// </summary>
        public Map Map
        {
            get { return _map; }
        }

        /// <summary>
        /// Gets the most recently pressed mouse button
        /// </summary>
        public MouseButtons MouseButton
        {
            get { return _mouseButton; }
        }

        /// <summary>
        /// Gets the currently selected Grh
        /// </summary>
        public Grh SelectedGrh
        {
            get { return _selectedGrh; }
        }

        /// <summary>
        /// Gets or sets the selected transformation box
        /// </summary>
        public TransBox SelectedTransBox
        {
            get { return _selTransBox; }
            set { _selTransBox = value; }
        }

        /// <summary>
        /// Gets the SpriteBatch used for rendering
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return _sb; }
        }

        /// <summary>
        /// Gets the SpriteFont used to draw text to the screen
        /// </summary>
        public SpriteFont SpriteFont
        {
            get { return _font; }
        }

        /// <summary>
        /// Gets the transformation box created
        /// </summary>
        public List<TransBox> TransBoxes
        {
            get { return _transBoxes; }
        }

        public WallCursor WallCursor
        {
            get { return _wallCursor; }
        }

        public ScreenForm()
        {
            InitializeComponent();
            GameScreen.Screen = this;

            // Load the settings
            LoadSettings();
            _mapGrhWalls = new MapGrhWalls(ContentPaths.Dev.Data.Join("grhdatawalls.xml"));

            // Create the world
            _world = new World(this, _camera);
        }

        void BeginEditGrhData(TreeNode node, GrhData gd)
        {
            if (_editNode != null || gd == null)
                return;

            _editNode = node;
            _editGrhData = gd;

            Vector2 pos;
            try
            {
                pos = new Vector2(_editGrhData.Width / 2f, _editGrhData.Height / 2f);
            }
            catch (ContentLoadException)
            {
                pos = Vector2.Zero;
            }
            _editGrhCamera.Zoom(pos, GameData.ScreenSize, 4f);
            _editGrh = new Grh(_editGrhData.GrhIndex, AnimType.Loop, GetTime());

            EditGrhForm f = new EditGrhForm(gd, _mapGrhWalls) { Location = new Point(0, 0) };
            f.FormClosed += EditGrhForm_Close;
            AddOwnedForm(f);
            f.Show();
            _editGrhWallItems = f.lstWalls.Items;
        }

        void btnTeleportCopy_Click(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            TeleportEntityBase selected = lstTeleports.SelectedItem as TeleportEntityBase;
            if (selected == null)
                return;

            _map.AddEntity(new TeleportEntity(selected.Position, selected.CB.Size, selected.Destination));
            UpdateTeleporterList();
        }

        void btnTeleportDelete_Click(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            TeleportEntityBase selected = lstTeleports.SelectedItem as TeleportEntityBase;
            if (selected == null)
                return;

            _map.RemoveEntity(selected);
            UpdateTeleporterList();
        }

        void btnTeleportLocate_Click(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            TeleportEntityBase selected = lstTeleports.SelectedItem as TeleportEntityBase;
            if (selected == null)
                return;

            Camera.Center(selected);
        }

        void btnTeleportNew_Click(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            _map.AddEntity(new TeleportEntity(new Vector2(10, 10), new Vector2(32, 32), new Vector2(10, 10)));
            UpdateTeleporterList();
        }

        void chkDrawEntities_CheckedChanged(object sender, EventArgs e)
        {
            Map.DrawEntities = chkDrawEntities.Checked;
        }

        void chkShowGrhs_CheckedChanged(object sender, EventArgs e)
        {
            Map.DrawForeground = chkShowGrhs.Checked;
            Map.DrawBackground = chkShowGrhs.Checked;
        }

        void chkShowWalls_CheckedChanged(object sender, EventArgs e)
        {
            Map.DrawWalls = chkShowWalls.Checked;
        }

        void cmbCurrWallType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_wallToEdit != null)
                _wallToEdit.CollisionType = (CollisionType)cmbCurrWallType.SelectedItem;
        }

        void cmdApplySize_Click(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            uint width;
            uint height;

            if (uint.TryParse(txtMapWidth.Text, out width) && uint.TryParse(txtMapHeight.Text, out height))
                _map.SetDimensions(new Vector2(width, height));

            txtMapWidth_TextChanged(null, null);
            txtMapHeight_TextChanged(null, null);
        }

        void cmdLoad_Click(object sender, EventArgs e)
        {
            using (FileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Load map";
                ofd.InitialDirectory = ContentPaths.Dev.Maps;
                ofd.RestoreDirectory = true;

                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.Cancel)
                    SetMap(ofd.FileName);
            }
        }

        void cmdNew_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show("Are you sure you wish to create a new map? All changes to the current map will be lost.",
                                "Create new map?", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            ushort index = Map.GetNextFreeIndex(ContentPaths.Dev);

            string newMapPath = ContentPaths.Dev.Maps.Join(index + "." + Map.MapFileSuffix);
            if (File.Exists(newMapPath))
            {
                MessageBox.Show(string.Format("Map.GetNextFreeIndex() returned the index [{0}] of an existing map!", index));
                return;
            }

            _map = new Map(index, _world, GameScreen.GraphicsDevice);
            _map.SetDimensions(new Vector2(30 * 32, 20 * 32));
            _map.Save(index, ContentPaths.Dev);
            SetMap(newMapPath);
        }

        void cmdSave_Click(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            Cursor = Cursors.WaitCursor;
            Enabled = false;

            // Add the MapGrh-bound walls
            var extraWalls = _mapGrhWalls.CreateWallList(_map.MapGrhs);
            foreach (WallEntity wall in extraWalls)
            {
                _map.AddEntity(wall);
            }

            // Save the map
            _map.Save(_map.Index, ContentPaths.Dev);

            // Remove the extra walls
            foreach (WallEntity wall in extraWalls)
            {
                _map.RemoveEntity(wall);
            }

            Enabled = true;
            Cursor = Cursors.Default;
        }

        public void DrawGame()
        {
            // Clear the background
            GameScreen.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.CornflowerBlue);

            // Draw a Grh if its being edited
            if (_editGrh != null)
            {
                DrawGrhPreview();
                return;
            }

            // Check for a valid map
            if (_map == null)
                return;

            // Begin the rendering
            _sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, _camera.Matrix);

            // Map
            _map.Draw(_sb, _camera);

            // MapGrh bound walls
            if (chkDrawAutoWalls.Checked)
            {
                foreach (MapGrh mg in _map.MapGrhs)
                {
                    if (!_camera.InView(mg.Grh, mg.Destination))
                        continue;

                    var boundWalls = _mapGrhWalls[mg.Grh.GrhData.GrhIndex];
                    if (boundWalls == null)
                        continue;

                    foreach (Wall wall in boundWalls)
                    {
                        EntityDrawer.Draw(_sb, wall, mg.Destination);
                    }
                }
            }

            // Border
            _mapBorderDrawer.Draw(_sb, _map, _camera);

            // Selection area
            if (_selectedTool != null)
                _selectedTool.DrawSelection(this);

            // Grid
            if (chkDrawGrid.Checked)
                _grid.Draw(_sb, _camera);

            // On-screen wall editor
            foreach (TransBox box in _transBoxes)
            {
                box.Draw(_sb);
            }

            // Selected Grh
            if (_selectedTool is AddGrhCursor && _selectedGrh.GrhData != null)
            {
                Vector2 drawPos;
                if (chkSnapGrhGrid.Checked)
                    drawPos = _grid.AlignDown(_cursorPos);
                else
                    drawPos = _cursorPos;

                // If we fail to draw the selected Grh, just ignore it
                try
                {
                    _selectedGrh.Draw(_sb, drawPos, _drawPreviewColor);
                }
// ReSharper disable EmptyGeneralCatchClause
                catch
// ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            // Tool interface
            if (_selectedTool != null)
                _selectedTool.DrawInterface(this);

            // End map rendering
            _sb.End();

            // Begin GUI rendering
            _sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

            // Cursor position
            Vector2 cursorPosText = new Vector2(GameScreen.Size.Width, GameScreen.Size.Height);
            cursorPosText -= new Vector2(100, 30);
            DrawShadedText(cursorPosText, _cursorPos.ToString(), Microsoft.Xna.Framework.Graphics.Color.White);

            // End GUI rendering and present
            _sb.End();
            GameScreen.GraphicsDevice.Present();
        }

        void DrawGrhPreview()
        {
            // Begin rendering
            _sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, _editGrhCamera.Matrix);

            // Grh - try/catch since invalid texture will throw an exception
#if !DEBUG
            try
            {
#endif
            _editGrh.Draw(_sb, Vector2.Zero);
#if !DEBUG
            }
            catch
            {
            }
#endif

            // Walls
            foreach (object o in _editGrhWallItems)
            {
                Wall wall = o as Wall;
                if (wall != null)
                    EntityDrawer.Draw(_sb, wall);
            }

            // End rendering
            _sb.End();
            GameScreen.GraphicsDevice.Present();
        }

        /// <summary>
        /// Draws text with a 1 pixel large black shading
        /// </summary>
        /// <param name="pos">Top-left corner to start drawing at</param>
        /// <param name="text">String to draw</param>
        /// <param name="color">Font color</param>
        public void DrawShadedText(Vector2 pos, string text, Microsoft.Xna.Framework.Graphics.Color color)
        {
            Microsoft.Xna.Framework.Graphics.Color shadeColor = Microsoft.Xna.Framework.Graphics.Color.Black;

            // Draw the shade
            _sb.DrawString(_font, text, pos - new Vector2(0, 1), shadeColor);
            _sb.DrawString(_font, text, pos - new Vector2(1, 0), shadeColor);
            _sb.DrawString(_font, text, pos + new Vector2(0, 1), shadeColor);
            _sb.DrawString(_font, text, pos + new Vector2(1, 0), shadeColor);

            // Draw the text
            _sb.DrawString(_font, text, pos, color);
        }

        void EditGrhForm_Close(object sender, FormClosedEventArgs e)
        {
            if (_editGrhData != null && _editNode != null)
            {
                _editNode.Remove();
                treeGrhs.UpdateGrh(_editGrhData);
            }

            _editNode = null;
            _editGrh = null;
            _editGrhData = null;
        }

        /// <summary>
        /// Finds which control has focus and no children controls
        /// </summary>
        /// <param name="control">Base control to check</param>
        /// <returns>Lowest-level control with focus</returns>
        static Control FindFocusControl(Control control)
        {
            Control ret = null;

            foreach (Control c in control.Controls)
            {
                if (c.Focused && c.Controls.Count == 0)
                {
                    ret = c;
                    break;
                }

                Control tmpRet = FindFocusControl(c);
                if (tmpRet != null)
                {
                    ret = tmpRet;
                    break;
                }
            }

            return ret;
        }

        void GameScreen_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseButton = e.Button;
            if (_map == null)
                return;

            GameScreen.Focus();

            if (_editGrh != null)
                return;

            // Forward to the corresponding tool's reaction to the screen's MouseDown
            _selectedTool.MouseDown(this, e);
        }

        void GameScreen_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseButton = e.Button;
            if (_map == null)
                return;

            // Update the cursor position
            _cursorPos = _camera.ToWorld(e.X, e.Y);

            if (_editGrh != null)
                return;

            // Forward to the corresponding tool's reaction to the screen's MouseMove
            _selectedTool.MouseMove(this, e);
        }

        void GameScreen_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseButton = e.Button;
            if (_map == null)
                return;

            if (_editGrh != null)
                return;

            // Forward to the corresponding tool's reaction to the screen's MouseUp
            _selectedTool.MouseUp(this, e);
        }

        string GetCategoryFromTreeNode(TreeNode node)
        {
            string category = "Uncategorized";

            // Check for a valid node
            if (node != null)
            {
                // Try and get the GrhData
                GrhData tmpGrhData = GrhTreeView.GetGrhData(treeGrhs.SelectedNode);

                if (tmpGrhData != null)
                {
                    // GrhData found, so use the category from that
                    category = tmpGrhData.Category;
                }
                else if (treeGrhs.SelectedNode.Name.Length == 0)
                {
                    // No GrhData found, so if the node has no name (is a folder), use its path
                    category = treeGrhs.SelectedNode.FullPath.Replace(treeGrhs.PathSeparator, ".");
                }
            }

            return category;
        }

        static void HookFormKeyEvents(Control root, KeyEventHandler kehDown, KeyEventHandler kehUp)
        {
            foreach (Control c in root.Controls)
            {
                if (c.Controls.Count > 0)
                    HookFormKeyEvents(c, kehDown, kehUp);
                c.KeyDown += kehDown;
                c.KeyUp += kehUp;
            }
        }

        /// <summary>
        /// Checks if a key is valid to be forwarded
        /// </summary>
        static bool IsKeyToForward(Keys key)
        {
            switch (key)
            {
                case _cameraUp:
                case _cameraDown:
                case _cameraLeft:
                case _cameraRight:
                case Keys.Delete:
                    return true;

                default:
                    return false;
            }
        }

        void LoadEditor()
        {
            // Create the engine objects 
            _content = new ContentManager(GameScreen.Services, ContentPaths.Build.Root);
            _sb = new SpriteBatch(GameScreen.GraphicsDevice);

            // Font
            _font = _content.Load<SpriteFont>(ContentPaths.Build.Fonts.Join("Game"));

            // Load the Grh information
            GrhInfo.Load(ContentPaths.Dev.Data.Join("grhdata.xml"), _content);
            treeGrhs.Initialize();
            TransBox.Initialize(GrhInfo.GetData("System", "Move"), GrhInfo.GetData("System", "Resize"));

            //Hook GrhTreeView context menu click events
            treeGrhs.GrhContextMenuEditClick += treeGrhs_mnuEdit;
            treeGrhs.GrhContextMenuDuplicateClick += treeGrhs_mnuDuplicate;
            treeGrhs.GrhContextMenuBatchChangeTextureClick += treeGrhs_mnuBatchChangeTexture;
            treeGrhs.GrhContextMenuNewGrhClick += treeGrhs_mnuNewGrh;

            // Start the stopwatch for the elapsed time checking
            _stopWatch.Start();

            // Set the wall types
            cmbWallType.Items.Clear();
            cmbCurrWallType.Items.Clear();
            foreach (CollisionType item in Enum.GetValues(typeof(CollisionType)))
            {
                cmbWallType.Items.Add(item);
                cmbCurrWallType.Items.Add(item);
            }
            cmbWallType.SelectedItem = CollisionType.Full;

            // Hook the toolbar visuals
            foreach (PictureBox pic in panToolBar.Controls)
            {
                pic.MouseClick += toolBarItem_Click;
            }
            toolBarItem_Click(picToolSelect, null);

            // Hook all controls to forward camera movement keys Form
            KeyEventHandler kehDown = OnKeyDownForward;
            KeyEventHandler kehUp = OnKeyUpForward;
            HookFormKeyEvents(this, kehDown, kehUp);

            // Load the first map
            SetMap(ContentPaths.Dev.Maps.Join("1." + Map.MapFileSuffix));
        }

        /// <summary>
        /// Loads the map editor settings
        /// </summary>
        void LoadSettings()
        {
            if (!File.Exists("MapEditorSettings.xml"))
                return;

            var data = XmlInfoReader.ReadFile("MapEditorSettings.xml");
            if (data == null)
                return;

            foreach (var d in data)
            {
                foreach (string key in d.Keys)
                {
                    string value;
                    if (!d.TryGetValue(key, out value))
                        continue;

                    switch (key)
                    {
                        case "Grid.Width":
                            txtGridWidth.Text = value;
                            break;
                        case "Grid.Height":
                            txtGridHeight.Text = value;
                            break;
                        case "Walls.SnapWallsToWalls":
                            chkSnapWallWall.Checked = bool.Parse(value);
                            break;
                        case "Walls.SnapWallsToGrid":
                            chkSnapWallGrid.Checked = bool.Parse(value);
                            break;
                        case "Display.Grid":
                            chkDrawGrid.Checked = bool.Parse(value);
                            break;
                        case "Display.Grhs":
                            chkShowGrhs.Checked = bool.Parse(value);
                            break;
                        case "Display.Walls":
                            chkShowWalls.Checked = bool.Parse(value);
                            break;
                        case "Display.AutoWalls":
                            chkDrawAutoWalls.Checked = bool.Parse(value);
                            break;
                        case "Display.Entities":
                            chkDrawEntities.Checked = bool.Parse(value);
                            break;
                    }
                }
            }
        }

        void lstSelectedWalls_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            WallCursor.SelectedWalls.Clear();
            foreach (ListboxWall lbw in lstSelectedWalls.SelectedItems)
            {
                WallCursor.SelectedWalls.Add(lbw);
            }
        }

        void lstTeleports_SelectedIndexChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
            {
                gbSelectedTeleporter.Enabled = false;
                return;
            }

            gbSelectedTeleporter.Enabled = true;
            txtTeleportX.Text = tele.Position.X.ToString();
            txtTeleportY.Text = tele.Position.Y.ToString();
            txtTeleportToX.Text = tele.Destination.X.ToString();
            txtTeleportToY.Text = tele.Destination.Y.ToString();
            txtTeleportWidth.Text = tele.CB.Width.ToString();
            txtTeleportHeight.Text = tele.CB.Height.ToString();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            Control focusControl = FindFocusControl(this);
            if (focusControl != null && focusControl.GetType() == typeof(TextBox))
                return;

            Vector2 startMoveCamera = new Vector2(_moveCamera.X, _moveCamera.Y);

            switch (e.KeyCode)
            {
                case _cameraUp:
                    _moveCamera.Y = -_cameraMoveRate;
                    break;
                case _cameraRight:
                    _moveCamera.X = _cameraMoveRate;
                    break;
                case _cameraDown:
                    _moveCamera.Y = _cameraMoveRate;
                    break;
                case _cameraLeft:
                    _moveCamera.X = -_cameraMoveRate;
                    break;

                case Keys.Delete:
                    if (_selectedTool != null)
                        _selectedTool.PressDelete(this);
                    _selTransBox = null;
                    _transBoxes.Clear();
                    break;
            }

            if (startMoveCamera != _moveCamera)
                e.Handled = true;
        }

        /// <summary>
        /// Forwards special KeyDown events to the form
        /// </summary>
        void OnKeyDownForward(object sender, KeyEventArgs e)
        {
            _keyEventArgs = e;
            if (IsKeyToForward(e.KeyCode))
                OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Vector2 startMoveCamera = new Vector2(_moveCamera.X, _moveCamera.Y);

            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                case Keys.S:
                case Keys.Down:
                    _moveCamera.Y = 0;
                    break;

                case Keys.Right:
                case Keys.D:
                case Keys.A:
                case Keys.Left:
                    _moveCamera.X = 0;
                    break;
            }

            if (startMoveCamera != _moveCamera)
                e.Handled = true;

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Forwards special KeyUp events to the form
        /// </summary>
        void OnKeyUpForward(object sender, KeyEventArgs e)
        {
            _keyEventArgs = e;
            if (IsKeyToForward(e.KeyCode))
                OnKeyUp(e);
        }

        void picToolGrhsAdd_Click(object sender, EventArgs e)
        {
            tcMenu.SelectTab(tabPageGrhs);
        }

        /// <summary>
        /// Saves the map editor settings
        /// </summary>
        void SaveSettings()
        {
            using (FileStream stream = new FileStream("MapEditorSettings.xml", FileMode.Create))
            {
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                using (XmlWriter w = XmlWriter.Create(stream, settings))
                {
                    if (w == null)
                        throw new Exception("Failed to create XmlWriter to save settings.");

                    w.WriteStartDocument();
                    w.WriteStartElement("Settings");

                    w.WriteStartElement("Walls");
                    w.WriteElementString("SnapWallsToWalls", chkSnapWallWall.Checked.ToString());
                    w.WriteElementString("SnapWallsToGrid", chkSnapWallGrid.Checked.ToString());
                    w.WriteEndElement();

                    w.WriteStartElement("Grid");
                    w.WriteElementString("Width", _grid.Width.ToString());
                    w.WriteElementString("Height", _grid.Height.ToString());
                    w.WriteEndElement();

                    w.WriteStartElement("Display");
                    w.WriteElementString("Grid", chkDrawGrid.Checked.ToString().ToLower());
                    w.WriteElementString("Grhs", chkShowGrhs.Checked.ToString().ToLower());
                    w.WriteElementString("Walls", chkShowWalls.Checked.ToString().ToLower());
                    w.WriteElementString("AutoWalls", chkDrawAutoWalls.Checked.ToString().ToLower());
                    w.WriteElementString("Entities", chkDrawEntities.Checked.ToString().ToLower());
                    w.WriteEndElement();

                    w.WriteEndElement();
                    w.WriteEndDocument();
                    w.Flush();
                }
            }
        }

        void ScreenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GrhInfo.Save(ContentPaths.Dev.Data.Join("grhdata.xml"));
            SaveSettings();
        }

        void ScreenForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadEditor();
            }
            catch (Exception ex)
            {
                // Stupid hack we have to do to get the exceptions to even show at all from this event
                string errmsg = "Exception: " + ex;
                Debug.Fail(errmsg);
                MessageBox.Show(errmsg);
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Sets the map being used. Use this instead of directly setting _map.
        /// </summary>
        /// <param name="filePath">Path to the map to use</param>
        void SetMap(string filePath)
        {
            ushort index = Map.GetIndexFromPath(filePath);
            _map = new Map(index, _world, GameScreen.GraphicsDevice);
            _map.Load(ContentPaths.Dev);

            // Remove all of the walls previously created from the MapGrhs
            var grhWalls = _mapGrhWalls.CreateWallList(_map.MapGrhs);
            var dupeWalls = _map.FindDuplicateWalls(grhWalls);
            foreach (WallEntity dupeWall in dupeWalls)
            {
                _map.RemoveEntity(dupeWall);
            }

            // Reset some of the variables
            _camera.Min = Vector2.Zero;
            txtMapWidth.Text = _map.Width.ToString();
            txtMapHeight.Text = _map.Height.ToString();
            UpdateTeleporterList();
        }

        void SetWallToEdit(WallEntity wall)
        {
            _wallToEdit = wall;
            gbCurrentWall.Enabled = (_wallToEdit != null);

            if (_wallToEdit == null)
                return;

            cmbCurrWallType.SelectedItem = _wallToEdit.CollisionType;
        }

        void tabPageGrhs_Enter(object sender, EventArgs e)
        {
            treeGrhs.Select();
        }

        internal void toolBarItem_Click(object sender, EventArgs e)
        {
            // Set the background colors for the tools
            foreach (PictureBox pic in panToolBar.Controls)
            {
                pic.BackColor = Color.White;
                pic.BorderStyle = BorderStyle.None;
            }

            PictureBox src = (PictureBox)sender;
            if (src == null)
                return;

            src.BackColor = Color.LightGreen;
            src.BorderStyle = BorderStyle.FixedSingle;

            // Set the selected tool
            if (src == picToolGrhs)
                _selectedTool = GrhCursor;
            else if (src == picToolSelect)
                _selectedTool = EntityCursor;
            else if (src == picToolGrhsAdd)
                _selectedTool = AddGrhCursor;
            else if (src == picToolWalls)
                _selectedTool = WallCursor;
            else if (src == picToolWallsAdd)
                _selectedTool = AddWallCursor;

            // Clear some selection stuff
            WallCursor.SelectedWalls.Clear();
            _transBoxes.Clear();
            _selTransBox = null;
        }

        void treeGrhs_DoubleClickGrh(object sender, GrhTreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                BeginEditGrhData(e.Node, e.GrhData);
        }

        void treeGrhs_mnuBatchChangeTexture(object sender, EventArgs e)
        {
            TreeNode node = treeGrhs.SelectedNode;
            if (node == null)
                return;

            // Show the new form
            BatchRenameTextureForm frm = new BatchRenameTextureForm(node, _content);

            // Disable this form until the rename one closes
            Enabled = false;
            frm.FormClosed += delegate
                              {
                                  Enabled = true;
                              };
            frm.Show();
        }

        void treeGrhs_mnuDuplicate(object sender, EventArgs e)
        {
            TreeNode node = treeGrhs.SelectedNode;

            // Confirm the duplicate request
            string text = string.Format("Are you sure you wish to duplicate these {0} nodes?", GrhTreeView.NodeCount(node));
            if (MessageBox.Show(text, "Duplicate nodes?", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            treeGrhs.DuplicateNodes(node);
        }

        void treeGrhs_mnuEdit(object sender, EventArgs e)
        {
            TreeNode node = treeGrhs.SelectedNode;
            GrhData gd = GrhTreeView.GetGrhData(node);

            if (node == null)
                return;

            if (gd != null && node.Nodes.Count == 0)
            {
                // Grh node
                BeginEditGrhData(node, gd);
            }
            else if (gd == null)
            {
                // Folder node
                node.BeginEdit();
            }
        }

        void treeGrhs_mnuNewGrh(object sender, EventArgs e)
        {
            // Create the new GrhData
            string category = GetCategoryFromTreeNode(treeGrhs.SelectedNode);
            var gd = GrhInfo.CreateGrhData(_content, category);
            treeGrhs.UpdateGrh(gd);

            // Begin edit
            var nodes = treeGrhs.Nodes.Find(gd.GrhIndex.ToString(), true);
            if (nodes.Length == 0)
            {
                Debug.Fail("Failed to find node.");
                return;
            }

            BeginEditGrhData(nodes[0], gd);
        }

        void treeGrhs_SelectGrh(object sender, GrhTreeViewEventArgs e)
        {
            if (_selectedGrh.GrhData == null || e.GrhData.GrhIndex != _selectedGrh.GrhData.GrhIndex)
            {
                _selectedGrh.SetGrh(e.GrhData.GrhIndex, AnimType.Loop, _currentTime);
                toolBarItem_Click(picToolGrhsAdd, null);
                picToolGrhsAdd_Click(this, null);
            }
        }

        void txtGridHeight_TextChanged(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            float result;
            if (float.TryParse(txtGridHeight.Text, out result))
                _grid.Height = result;
        }

        void txtGridWidth_TextChanged(object sender, EventArgs e)
        {
            if (_map == null)
                return;

            float result;
            if (float.TryParse(txtGridWidth.Text, out result))
                _grid.Width = result;
        }

        void txtMapHeight_TextChanged(object sender, EventArgs e)
        {
            uint o;
            if (uint.TryParse(txtMapHeight.Text, out o))
            {
                if (o == _map.Height)
                    txtMapHeight.BackColor = _colorNormal;
                else
                    txtMapHeight.BackColor = _colorChanged;
            }
            else
                txtMapHeight.BackColor = _colorError;
        }

        void txtMapWidth_TextChanged(object sender, EventArgs e)
        {
            uint o;
            if (uint.TryParse(txtMapWidth.Text, out o))
            {
                if (o == _map.Width)
                    txtMapWidth.BackColor = _colorNormal;
                else
                    txtMapWidth.BackColor = _colorChanged;
            }
            else
                txtMapWidth.BackColor = _colorError;
        }

        void txtTeleportHeight_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            float height;
            if (!float.TryParse(txtTeleportHeight.Text, out height))
                txtTeleportHeight.BackColor = _colorError;
            else
            {
                txtTeleportHeight.BackColor = _colorNormal;
                tele.Resize(new Vector2(tele.Size.X, height));
                UpdateTeleporterList();
            }
        }

        void txtTeleportMap_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            // TODO: Check for a valid map, not just a valid number
            ushort mapID;
            if (!ushort.TryParse(txtTeleportX.Text, out mapID))
                txtTeleportX.BackColor = _colorError;
            else
            {
                txtTeleportX.BackColor = _colorNormal;
                tele.DestinationMap = mapID;
                UpdateTeleporterList();
            }
        }

        void txtTeleportToX_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            float x;
            if (!float.TryParse(txtTeleportToX.Text, out x))
                txtTeleportToX.BackColor = _colorError;
            else
            {
                txtTeleportToX.BackColor = _colorNormal;
                tele.Destination = new Vector2(x, tele.Destination.Y);
                UpdateTeleporterList();
            }
        }

        void txtTeleportToY_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            float y;
            if (!float.TryParse(txtTeleportToY.Text, out y))
                txtTeleportToY.BackColor = _colorError;
            else
            {
                txtTeleportToY.BackColor = _colorNormal;
                tele.Destination = new Vector2(tele.Destination.X, y);
                UpdateTeleporterList();
            }
        }

        void txtTeleportWidth_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            float width;
            if (!float.TryParse(txtTeleportWidth.Text, out width))
                txtTeleportWidth.BackColor = _colorError;
            else
            {
                txtTeleportWidth.BackColor = _colorNormal;
                tele.Resize(new Vector2(width, tele.Size.Y));
                UpdateTeleporterList();
            }
        }

        void txtTeleportX_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            float x;
            if (!float.TryParse(txtTeleportX.Text, out x))
                txtTeleportX.BackColor = _colorError;
            else
            {
                txtTeleportX.BackColor = _colorNormal;
                _map.SafeTeleportEntity(tele, new Vector2(x, tele.Position.Y));
                UpdateTeleporterList();
            }
        }

        void txtTeleportY_TextChanged(object sender, EventArgs e)
        {
            TeleportEntityBase tele = lstTeleports.SelectedItem as TeleportEntityBase;
            if (tele == null)
                return;

            float y;
            if (!float.TryParse(txtTeleportY.Text, out y))
                txtTeleportY.BackColor = _colorError;
            else
            {
                txtTeleportY.BackColor = _colorNormal;
                _map.SafeTeleportEntity(tele, new Vector2(tele.Position.X, y));
                UpdateTeleporterList();
            }
        }

        /// <summary>
        /// Updates the cursor based on the transformation box the cursor is over
        /// or the currently selected transformation box
        /// </summary>
        void UpdateCursor()
        {
            // Don't do anything if we have an unknown cursor
            if (Cursor != Cursors.Default && Cursor != Cursors.SizeAll && Cursor != Cursors.SizeNESW && Cursor != Cursors.SizeNS &&
                Cursor != Cursors.SizeNWSE && Cursor != Cursors.SizeWE)
                return;

            if (_selectedTool != null)
                _selectedTool.UpdateCursor(this);

            // Set to default if it wasn't yet set
            Cursor = Cursors.Default;
        }

        public void UpdateGame()
        {
            // Update the time
            _currentTime = (int)_stopWatch.ElapsedMilliseconds;

            // Edited Grh
            if (_editGrh != null)
                _editGrh.Update(_currentTime);

            // Check for a map
            if (_map == null)
                return;

            // Move the camera
            _camera.Min += _moveCamera;

            // Update the map
            _map.Update();

            // Update the cursor
            UpdateCursor();

            // Update the selected grh
            _selectedGrh.Update(_currentTime);
        }

        /// <summary>
        /// Updates the list of selected walls
        /// </summary>
        public void UpdateSelectedWallsList(List<WallEntity> selectedWalls)
        {
            lstSelectedWalls.Items.Clear();
            foreach (WallEntity wall in selectedWalls)
            {
                lstSelectedWalls.Items.Add(new ListboxWall(wall));
            }

            tcMenu.SelectTab(tabPageWalls);
            lstSelectedWalls.Focus();

            for (int i = 0; i < lstSelectedWalls.Items.Count; i++)
            {
                lstSelectedWalls.SetSelected(i, true);
            }

            if (selectedWalls.Count == 1)
                SetWallToEdit(selectedWalls[0]);
            else
                SetWallToEdit(null);
        }

        public void UpdateTeleporterList()
        {
            if (_map == null)
                return;

            object selected = lstTeleports.SelectedItem;

            lstTeleports.Items.Clear();

            foreach (Entity entity in _map.Entities)
            {
                if (entity is TeleportEntityBase)
                    lstTeleports.Items.Add(entity);
            }

            if (selected != null)
                lstTeleports.SelectedItem = selected;
        }

        #region IGetTime Members

        /// <summary>
        /// Gets the current game time where time 0 is when the application started
        /// </summary>
        /// <returns>Current game time in milliseconds</returns>
        public int GetTime()
        {
            return _currentTime;
        }

        #endregion
    }
}