﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using SFML.Graphics;
using SFML.Window;

namespace NetGore.Graphics
{
    /// <summary>
    /// Base class for a container for the game screen.
    /// </summary>
    public abstract class GameBase : IGameContainer
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly Point _fullscreenRes;
        readonly Point _windowedRes;

        IntPtr _displayHandle;
        bool? _changeIsFullscreenValue;
        bool _isDisposed;
        bool _isFullscreen;
        RenderWindow _renderWindow;
        bool _showMouseCursor;
        bool _useVerticalSync;

        public IntPtr DisplayHandle
        {
            get
            {
                return _displayHandle;
            }
            set
            {
                if (_displayHandle == value)
                    return;

                _displayHandle = value;

                if (!IsFullscreen)
                    SwitchToWindowed(true);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBase"/> class.
        /// </summary>
        /// <param name="displayHandle">The display handle. Use <see cref="IntPtr.Zero"/> when you want to create a separate window,
        /// or use the <see cref="IntPtr"/> for the handle of the control to display on.</param>
        /// <param name="windowedResolution">The windowed resolution.</param>
        /// <param name="fullscreenResolution">The fullscreen resolution.</param>
        protected GameBase(IntPtr displayHandle, Point windowedResolution, Point fullscreenResolution)
        {
            _displayHandle = displayHandle;
            _windowedRes = windowedResolution;
            _fullscreenRes = fullscreenResolution;

            SwitchToWindowed(true);
        }

        /// <summary>
        /// Handles switching to and from fullscreen mode using the <see cref="_changeIsFullscreenValue"/> value.
        /// This lets us ensure we switch to/from fullscreen only when we know it is a valid time to do so.
        /// </summary>
        void ChangeFullscreen()
        {
            // Check for a value
            if (!_changeIsFullscreenValue.HasValue)
                return;

            // Change mode
            if (_changeIsFullscreenValue.Value)
                SwitchToFullscreen();
            else
                SwitchToWindowed();

            // Clear value
            _changeIsFullscreenValue = null;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposeManaged"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManaged)
        {
            if (disposeManaged)
                RenderWindow = null;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="GameBase"/> is reclaimed by garbage collection.
        /// </summary>
        ~GameBase()
        {
            if (IsDisposed)
                return;

            _isDisposed = true;

            Dispose(false);
        }

        /// <summary>
        /// When overridden in the derived class, handles drawing the game.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        protected abstract void HandleDraw(TickCount currentTime);

        /// <summary>
        /// When overridden in the derived class, handles updating the game.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        protected abstract void HandleUpdate(TickCount currentTime);

        /// <summary>
        /// Handles the <see cref="IGameContainer.RenderWindowChanged"/> event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderWindowChanged(RenderWindow oldValue, RenderWindow newValue)
        {
            if (newValue == null)
                return;

            newValue.ShowMouseCursor(ShowMouseCursor);
            newValue.UseVerticalSync(UseVerticalSync);
        }

        /// <summary>
        /// Sets the event listeners on a <see cref="RenderWindow"/>.
        /// </summary>
        /// <param name="rw">The <see cref="RenderWindow"/> to set the event listeners on.</param>
        /// <param name="remove">If true, the event listeners will be removed. If false, they will be added.</param>
        void SetRenderWindowListeners(RenderWindow rw, bool remove)
        {
            // Check for a valid RenderWindow
            if (rw == null)
            {
                Debug.Fail("rw should not be null.");
                return;
            }

            if (!remove)
            {
                // When setting the events, also make sure they have been removed first to avoid duplicate listeners
                SetRenderWindowListeners(rw, true);

                // Add listeners
                rw.Closed += rw_Closed;
                rw.GainedFocus += rw_GainedFocus;
                rw.JoyButtonPressed += rw_JoyButtonPressed;
                rw.JoyButtonReleased += rw_JoyButtonReleased;
                rw.JoyMoved += rw_JoyMoved;
                rw.KeyPressed += rw_KeyPressed;
                rw.KeyReleased += rw_KeyReleased;
                rw.LostFocus += rw_LostFocus;
                rw.MouseButtonPressed += rw_MouseButtonPressed;
                rw.MouseButtonReleased += rw_MouseButtonReleased;
                rw.MouseEntered += rw_MouseEntered;
                rw.MouseLeft += rw_MouseLeft;
                rw.MouseMoved += rw_MouseMoved;
                rw.MouseWheelMoved += rw_MouseWheelMoved;
                rw.Resized += rw_Resized;
                rw.TextEntered += rw_TextEntered;
            }
            else
            {
                // Remove listeners
                rw.Closed -= rw_Closed;
                rw.GainedFocus -= rw_GainedFocus;
                rw.JoyButtonPressed -= rw_JoyButtonPressed;
                rw.JoyButtonReleased -= rw_JoyButtonReleased;
                rw.JoyMoved -= rw_JoyMoved;
                rw.KeyPressed -= rw_KeyPressed;
                rw.KeyReleased -= rw_KeyReleased;
                rw.LostFocus -= rw_LostFocus;
                rw.MouseButtonPressed -= rw_MouseButtonPressed;
                rw.MouseButtonReleased -= rw_MouseButtonReleased;
                rw.MouseEntered -= rw_MouseEntered;
                rw.MouseLeft -= rw_MouseLeft;
                rw.MouseMoved -= rw_MouseMoved;
                rw.MouseWheelMoved -= rw_MouseWheelMoved;
                rw.Resized -= rw_Resized;
                rw.TextEntered -= rw_TextEntered;
            }
        }

        /// <summary>
        /// Switches to fullscreen mode.
        /// </summary>
        /// <param name="force">If true, the <see cref="RenderWindow"/> will be recreated even if already in fullscreen mode.</param>
        void SwitchToFullscreen(bool force = false)
        {
            if (!force && _isFullscreen)
                return;

            if (log.IsInfoEnabled)
                log.Info("Changing to fullscreen mode.");

            RenderWindow = null;

            var videoMode = new VideoMode((uint)FullscreenResolution.X, (uint)FullscreenResolution.Y);
            var newRW = new RenderWindow(videoMode, "My game", Styles.Fullscreen); // TODO: !! Title

            _isFullscreen = true;

            RenderWindow = newRW;
        }

        /// <summary>
        /// Switches to windowed mode.
        /// </summary>
        /// <param name="force">If true, the <see cref="RenderWindow"/> will be recreated even if already in windowed mode.</param>
        void SwitchToWindowed(bool force = false)
        {
            if (!force && !_isFullscreen)
                return;

            if (log.IsInfoEnabled)
                log.Info("Changing to windowed mode.");

            RenderWindow = null;

            RenderWindow newRW;

            if (DisplayHandle == IntPtr.Zero)
            {
                // Not using custom handle
                var videoMode = new VideoMode((uint)WindowedResolution.X, (uint)WindowedResolution.Y);
                newRW = new RenderWindow(videoMode, "My game", Styles.Titlebar | Styles.Close); // TODO: !! Title
            }
            else
            {
                // Using custom handle
                newRW = new RenderWindow(DisplayHandle);
            }

            _isFullscreen = false;

            RenderWindow = newRW;
        }

        /// <summary>
        /// Handles the Closed event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rw_Closed(object sender, EventArgs e)
        {
            if (Closed != null)
                Closed(this, e);
        }

        /// <summary>
        /// Handles the GainedFocus event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rw_GainedFocus(object sender, EventArgs e)
        {
            if (GainedFocus != null)
                GainedFocus(this, e);
        }

        /// <summary>
        /// Handles the JoyButtonPressed event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.JoyButtonEventArgs"/> instance containing the event data.</param>
        void rw_JoyButtonPressed(object sender, JoyButtonEventArgs e)
        {
            if (JoyButtonPressed != null)
                JoyButtonPressed(this, e);
        }

        /// <summary>
        /// Handles the JoyButtonReleased event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.JoyButtonEventArgs"/> instance containing the event data.</param>
        void rw_JoyButtonReleased(object sender, JoyButtonEventArgs e)
        {
            if (JoyButtonReleased != null)
                JoyButtonReleased(this, e);
        }

        /// <summary>
        /// Handles the JoyMoved event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.JoyMoveEventArgs"/> instance containing the event data.</param>
        void rw_JoyMoved(object sender, JoyMoveEventArgs e)
        {
            if (JoyMoved != null)
                JoyMoved(this, e);
        }

        /// <summary>
        /// Handles the KeyPressed event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.KeyEventArgs"/> instance containing the event data.</param>
        void rw_KeyPressed(object sender, KeyEventArgs e)
        {
            if (KeyPressed != null)
                KeyPressed(this, e);
        }

        /// <summary>
        /// Handles the KeyReleased event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.KeyEventArgs"/> instance containing the event data.</param>
        void rw_KeyReleased(object sender, KeyEventArgs e)
        {
            if (KeyReleased != null)
                KeyReleased(this, e);
        }

        /// <summary>
        /// Handles the LostFocus event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rw_LostFocus(object sender, EventArgs e)
        {
            if (LostFocus != null)
                LostFocus(this, e);
        }

        /// <summary>
        /// Handles the MouseButtonPressed event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.MouseButtonEventArgs"/> instance containing the event data.</param>
        void rw_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (MouseButtonPressed != null)
                MouseButtonPressed(this, e);
        }

        /// <summary>
        /// Handles the MouseButtonReleased event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.MouseButtonEventArgs"/> instance containing the event data.</param>
        void rw_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (MouseButtonReleased != null)
                MouseButtonReleased(this, e);
        }

        /// <summary>
        /// Handles the MouseEntered event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rw_MouseEntered(object sender, EventArgs e)
        {
            if (MouseEntered != null)
                MouseEntered(this, e);
        }

        /// <summary>
        /// Handles the MouseLeft event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void rw_MouseLeft(object sender, EventArgs e)
        {
            if (MouseLeft != null)
                MouseLeft(this, e);
        }

        /// <summary>
        /// Handles the MouseMoved event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.MouseMoveEventArgs"/> instance containing the event data.</param>
        void rw_MouseMoved(object sender, MouseMoveEventArgs e)
        {
            if (MouseMoved != null)
                MouseMoved(this, e);
        }

        /// <summary>
        /// Handles the MouseWheelMoved event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.MouseWheelEventArgs"/> instance containing the event data.</param>
        void rw_MouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            if (MouseWheelMoved != null)
                MouseWheelMoved(this, e);
        }

        /// <summary>
        /// Handles the Resized event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.SizeEventArgs"/> instance containing the event data.</param>
        void rw_Resized(object sender, SizeEventArgs e)
        {
            if (Resized != null)
                Resized(this, e);
        }

        /// <summary>
        /// Handles the TextEntered event of the <see cref="GameBase.RenderWindow"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SFML.Window.TextEventArgs"/> instance containing the event data.</param>
        void rw_TextEntered(object sender, TextEventArgs e)
        {
            if (TextEntered != null)
                TextEntered(this, e);
        }

        #region IGameContainer Members

        /// <summary>
        /// Event handler for the Closed event.
        /// </summary>
        public event EventHandler Closed = null;

        /// <summary>
        /// Event handler for the GainedFocus event.
        /// </summary>
        public event EventHandler GainedFocus = null;

        /// <summary>
        /// Event handler for the JoyButtonPressed event.
        /// </summary>
        public event EventHandler<JoyButtonEventArgs> JoyButtonPressed = null;

        /// <summary>
        /// Event handler for the JoyButtonReleased event.
        /// </summary>
        public event EventHandler<JoyButtonEventArgs> JoyButtonReleased = null;

        /// <summary>
        /// Event handler for the JoyMoved event.
        /// </summary>
        public event EventHandler<JoyMoveEventArgs> JoyMoved = null;

        /// <summary>
        /// Event handler for the KeyPressed event.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyPressed = null;

        /// <summary>
        /// Event handler for the KeyReleased event.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyReleased = null;

        /// <summary>
        /// Event handler for the LostFocus event.
        /// </summary>
        public event EventHandler LostFocus = null;

        /// <summary>
        /// Event handler for the MouseButtonPressed event.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonPressed = null;

        /// <summary>
        /// Event handler for the MouseButtonReleased event.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonReleased = null;

        /// <summary>
        /// Event handler for the MouseEntered event.
        /// </summary>
        public event EventHandler MouseEntered = null;

        /// <summary>
        /// Event handler for the MouseLeft event.
        /// </summary>
        public event EventHandler MouseLeft = null;

        /// <summary>
        /// Event handler for the MouseMoved event.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseMoved = null;

        /// <summary>
        /// Event handler for the MouseWheelMoved event.
        /// </summary>
        public event EventHandler<MouseWheelEventArgs> MouseWheelMoved = null;

        public event GameContainerPropertyChangedEventHandler<RenderWindow> RenderWindowChanged;

        /// <summary>
        /// Event handler for the Resized event.
        /// </summary>
        public event EventHandler<SizeEventArgs> Resized = null;

        /// <summary>
        /// Event handler for the TextEntered event.
        /// </summary>
        public event EventHandler<TextEventArgs> TextEntered = null;

        /// <summary>
        /// Gets the resolution to use while in fullscreen mode.
        /// </summary>
        public Point FullscreenResolution
        {
            get { return _fullscreenRes; }
        }

        /// <summary>
        /// Gets if this object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        /// Gets or sets if fullscreen mode is enabled.
        /// </summary>
        public bool IsFullscreen
        {
            get { return _isFullscreen; }
            set { _changeIsFullscreenValue = value; }
        }

        /// <summary>
        /// Gets the current <see cref="IGameContainer.RenderWindow"/>. Can be null.
        /// </summary>
        public RenderWindow RenderWindow
        {
            get { return _renderWindow; }
            private set
            {
                // Check that the value is actually changing
                if (_renderWindow == value)
                    return;

                // Store old RenderWindow
                var oldRW = _renderWindow;

                // Set new RenderWindow
                _renderWindow = value;

                // Remove the event listeners from the old RenderWindow, and add to the new
                if (oldRW != null)
                    SetRenderWindowListeners(oldRW, true);

                if (_renderWindow != null)
                    SetRenderWindowListeners(_renderWindow, false);

                // Raise events
                OnRenderWindowChanged(oldRW, _renderWindow);
                if (RenderWindowChanged != null)
                    RenderWindowChanged(this, oldRW, _renderWindow);

                // Dispose old RenderWindow
                if (oldRW != null && !oldRW.IsDisposed)
                    oldRW.Dispose();
            }
        }

        /// <summary>
        /// Gets the size of the screen in pixels.
        /// </summary>
        public Vector2 ScreenSize
        {
            get
            {
                if (IsFullscreen)
                    return new Vector2(FullscreenResolution.X, FullscreenResolution.Y);
                else
                    return new Vector2(WindowedResolution.X, WindowedResolution.Y);
            }
        }

        /// <summary>
        /// Gets or sets if the mouse cursor is to be displayed.
        /// </summary>
        public bool ShowMouseCursor
        {
            get { return _showMouseCursor; }
            set
            {
                _showMouseCursor = value;

                if (RenderWindow != null)
                    RenderWindow.ShowMouseCursor(ShowMouseCursor);
            }
        }

        /// <summary>
        /// Gets or sets if vertical sync is to be used.
        /// </summary>
        public bool UseVerticalSync
        {
            get { return _useVerticalSync; }
            set
            {
                _useVerticalSync = value;

                if (RenderWindow != null)
                    RenderWindow.UseVerticalSync(UseVerticalSync);
            }
        }

        /// <summary>
        /// Gets the resolution to use while in windowed mode.
        /// </summary>
        public Point WindowedResolution
        {
            get { return _windowedRes; }
        }

        bool _willDispose;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Make sure we are not already disposed
            if (IsDisposed)
                return;

            // If handling a frame (which is common since dispose events frequently come from input events), then we will wait until
            // the frame is over before disposing
            if (_isHandlingFrame)
            {
                _willDispose = true;
                return;
            }

            // Dispose
            _isDisposed = true;

            GC.SuppressFinalize(this);

            Dispose(true);
        }

        bool _isHandlingFrame;

        /// <summary>
        /// Handles processing and drawing a single frame of the game. This needs to be called continually in a loop to keep a fluent
        /// stream of updates.
        /// </summary>
        public void HandleFrame()
        {
            if (IsDisposed)
                return;

            _isHandlingFrame = false;

            // Check if we need to dispose
            if (_willDispose)
            {
                Dispose();
                return;
            }

            // Begin handling frame
            _isHandlingFrame = true;

            try
            {
                // Check if we need to toggle fullscreen mode
                ChangeFullscreen();

                // Get the current time
                var currentTime = TickCount.Now;

                // Dispatch the events
                var rw = RenderWindow;
                if (rw != null && !rw.IsDisposed)
                    rw.DispatchEvents();

                // Update
                HandleUpdate(currentTime);

                // Draw
                rw = RenderWindow;
                if (rw != null && !rw.IsDisposed)
                    HandleDraw(currentTime);

                // Display
                rw = RenderWindow;
                if (rw != null && !rw.IsDisposed)
                    rw.Display();
            }
            finally
            {
                // End handling frame
                _isHandlingFrame = false;
            }

            // Check if we need to dispose
            if (_willDispose)
            {
                Dispose();
                return;
            }
        }

        #endregion
    }
}