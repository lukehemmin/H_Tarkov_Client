using System.Windows;
using System.Windows.Controls;

namespace Aki.Launcher.Custom_Controls
{
    /// <summary>
    /// Interaction logic for MenuBarButton.xaml
    /// </summary>
    public partial class MenuBarButton : Button
    {
        public MenuBarButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MenuBarButton), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
    }
}
