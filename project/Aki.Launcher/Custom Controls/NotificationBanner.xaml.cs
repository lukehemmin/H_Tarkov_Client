/* NotificationBanner.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */


using Aki.Launcher.Models.Launcher.Notifications;
using System.Windows;
using System.Windows.Controls;

namespace Aki.Launcher.Custom_Controls
{
    /// <summary>
    /// Interaction logic for NotificationBanner.xaml
    /// </summary>
    public partial class NotificationBanner : UserControl
    {
        public NotificationBanner()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NotificationQueueProperty =
            DependencyProperty.Register("NotificationQueue", typeof(NotificationQueue), typeof(NotificationBanner), new PropertyMetadata(null));

        public NotificationQueue NotificationQueue
        {
            get => (NotificationQueue)GetValue(NotificationQueueProperty);
            set => SetValue(NotificationQueueProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationQueue.queue.Count > 0 && NotificationQueue.queue[0] != null)
            {
                NotificationQueue.queueTimer.Stop();
                NotificationQueue.queue[0].ItemAction?.Invoke();
                NotificationQueue.Next(true);
            }
        }
    }
}
