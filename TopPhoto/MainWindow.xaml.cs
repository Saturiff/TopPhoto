using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace TopPhoto
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            imageWidget = new RenderComponent(this, img0, AskFilePath());
        }

        private RenderComponent imageWidget;

        private string AskFilePath()
        {
            Microsoft.Win32.OpenFileDialog dlg;
            bool? result = null;
            do
            {
                dlg = new Microsoft.Win32.OpenFileDialog();
                result = dlg.ShowDialog();
            }
            while (result != true);
            return dlg.FileName;
        }

        #region 鍵盤/滑鼠與程式間的交互

        bool isManipulatable = false;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            bool isKeyDown = ((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0) || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0);
            isManipulatable = (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0) || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow;
            if (isKeyDown)
            {
                ToggleManipulateWindow(KeyStates.Down);
            }
            else if (isManipulatable)
            {
                ToggleManipulateWindow(KeyStates.None);
            }
        }

        private IntPtr hwnd;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetOriStyle(hwnd);
        }

        private bool canManipulateWindow = false;

        private bool isMouseEntered = false;

        private KeyStates gKeyStates = KeyStates.None;

        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            /* None -> Down, Down -> None : 改變可操縱視窗狀態並保存目前狀態
             * None -> None, Down -> Down : 不做任何事
             */
            if (inKeyStates != gKeyStates) // 狀態改變則致能
            {
                canManipulateWindow = (canManipulateWindow) ? false : true;
                WindowsServices.SetWindowExTransparent(hwnd);
                gKeyStates = inKeyStates;
            }
        }

        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ChangeSize(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) // 滾輪向上放大字體，反之縮小字體
            {
                imageWidget.Bigger();
            }
            else
            {
                imageWidget.Smaller();
            }
        }

        #endregion

        private void EventMouseEnter(object sender, MouseEventArgs e) => isMouseEntered = true;

        private void EventMouseLeave(object sender, MouseEventArgs e) => isMouseEntered = false;

        private void EventMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed && isManipulatable && isMouseEntered)
            {
                Close();
                Environment.Exit(Environment.ExitCode);
            }
        }
    }
}
