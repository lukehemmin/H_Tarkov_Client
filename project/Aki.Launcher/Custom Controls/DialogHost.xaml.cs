/* DialogHost.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using Aki.Launcher.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shell;

namespace Aki.Launcher.Custom_Controls
{
    /// <summary>
    /// Interaction logic for DialogHost.xaml
    /// </summary>
    public partial class DialogHost : UserControl
    {
        public DialogHost()
        {
            InitializeComponent();

            //set the visibility to hidden, so the dialog host isn't blocking any content under it.
            backdrop.Visibility = Visibility.Hidden;
        }

        //results we will return.
        private static object Results = null;

        //a property to coordinate when results have been returned from our dialog window.
        private static bool ResultsReady = false;


        private static bool CanOpenNextDialog = true;

        private static void SetDialogPosistion(Window dialog)
        {
            //set the position of the dialog window to be centered in our mainwindow.
            dialog.Top = Application.Current.MainWindow.Top + (Application.Current.MainWindow.ActualHeight - dialog.ActualHeight) / 2;
            dialog.Left = Application.Current.MainWindow.Left + (Application.Current.MainWindow.ActualWidth - dialog.ActualWidth) / 2;
        }

        /// <summary>
        /// The default dialog host name to use
        /// </summary>
        public static string DefaultHost = "";

        /// <summary>
        /// Shows a ViewModel or UserControl inside a dialog window and waits for a result to be returned.
        /// </summary>
        /// <param name="HostName">The x:Name of the dialog host you want to use to display this dialog</param>
        /// <param name="DialogViewModel">The ViewModel or UserControl to load</param>
        /// <returns>The result of the dialog</returns>
        public static async Task<object> ShowDialog(IReturnDialogResult DialogViewModel, string HostName = "")
        {
            if (HostName == "")
            {
                HostName = DefaultHost;
            }

            //create a temporary host
            DialogHost host = null;

            //clear any static data
            ResultsReady = false;
            Results = null;

            //find the dialog host by name
            object frameworkElement = Application.Current.MainWindow.FindName(HostName);
            if (frameworkElement is DialogHost dh)
            {
                //set our temp host
                host = dh;
            }

            if (host == null)
            {
                //no dialog found
                return null;
            }

            //run the backdrop open animation
            await OpenDialogHost(host);

            //subscribe to the needed event handlers to keep the dialog window centered during main window move / main window resize
            Application.Current.MainWindow.LocationChanged += MainWindow_LocationChanged;
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;

            //subscribe to the provided viewmodels results ready event handler. Fires when results have been returned from the viewmodel.
            DialogViewModel.ResultsReady += DialogViewModel_ResultsReady;

            //create and setup our new dialog window for the provided viewmodel
            Window dialogWindow = new Window();
            dialogWindow.Owner = Application.Current.MainWindow;
            dialogWindow.Content = DialogViewModel;

            dialogWindow.WindowStyle = WindowStyle.SingleBorderWindow;

            dialogWindow.ResizeMode = ResizeMode.NoResize;
            dialogWindow.SizeToContent = SizeToContent.WidthAndHeight;
            dialogWindow.ShowInTaskbar = false;

            //loaded event for animations
            dialogWindow.Loaded += DialogWindow_Loaded;

            //we need to re-render the window if dynamic views are loaded; otherwise, the window may look weird. So we are going to invalidate the visuals as soon as the content loads, so it is rendered correctly.
            dialogWindow.ContentRendered += (s, e) => { dialogWindow.InvalidateVisual(); };

            //setting the windowchrome for the dialog window so we can have a nice clear dialog area. No title bar or frames. Also maintains drop shadow and open / close animations.
            WindowChrome.SetWindowChrome(dialogWindow, new WindowChrome
            {
                GlassFrameThickness = new Thickness(0),
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                ResizeBorderThickness = new Thickness(0),
                UseAeroCaptionButtons = false,
            });

            //shows the dialog window and set the posistion
            dialogWindow.Show();
            SetDialogPosistion(dialogWindow);

            //wait for a result from the dialog window.
            await Task.Run(() =>
            {
                do
                {

                }
                while (!ResultsReady);
            });

            //close the dialog window
            dialogWindow.Close();

            //unsubscribe from events
            Application.Current.MainWindow.LocationChanged -= MainWindow_LocationChanged;
            Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;

            //run backdrop close animation
            CloseDialogHost(host);

            //return results
            return Results;
        }

        private static void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Window win in Application.Current.MainWindow.OwnedWindows)
            {
                SetDialogPosistion(win);
            }
        }

        private static void DialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DialogHost dh)
            {
                dh.Opacity = 0;
                DoubleAnimation doubleAnimation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
                dh.BeginAnimation(OpacityProperty, doubleAnimation);
            }
        }

        private static void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            foreach (Window win in Application.Current.MainWindow.OwnedWindows)
            {
                SetDialogPosistion(win);
            }
        }

        private static void DialogViewModel_ResultsReady(object sender, object e)
        {
            Results = e;
            ResultsReady = true;
        }

        private static async Task OpenDialogHost(DialogHost host)
        {
            await Task.Run(() =>
            {
                do
                {

                }
                while (!CanOpenNextDialog);
            });

            host.backdrop.Opacity = 0;
            host.backdrop.Visibility = Visibility.Visible;
            DoubleAnimation doubleAnimation = new DoubleAnimation(.7, TimeSpan.FromMilliseconds(250));
            host.backdrop.BeginAnimation(OpacityProperty, doubleAnimation);
        }

        private static void CloseDialogHost(DialogHost host)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));

            //event handler to make sure the animation has completed before changing the visibility.
            doubleAnimation.Completed += (s, e) =>
            {
                host.backdrop.Visibility = Visibility.Hidden;
                CanOpenNextDialog = true;
            };

            CanOpenNextDialog = false;
            host.backdrop.BeginAnimation(OpacityProperty, doubleAnimation);
        }
    }
}
