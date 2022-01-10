/* MenuBarControl.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using Aki.Launcher.Models.Launcher;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aki.Launcher.Custom_Controls
{
    /// <summary>
    /// Interaction logic for MenuBarControl.xaml
    /// </summary>
    public partial class MenuBarControl : UserControl
    {
        public MenuBarControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemCollectionProperty =
            DependencyProperty.Register("ItemCollection", typeof(ObservableCollection<MenuBarItem>), typeof(MenuBarControl), new PropertyMetadata(default));

        public ObservableCollection<MenuBarItem> ItemCollection
        {
            get => (ObservableCollection<MenuBarItem>)GetValue(ItemCollectionProperty);
            set => SetValue(ItemCollectionProperty, value);
        }

        public static readonly DependencyProperty ItemCollectionCommandProperty =
            DependencyProperty.Register("ItemCollectionCommand", typeof(ICommand), typeof(MenuBarControl), new PropertyMetadata(null));
        public ICommand ItemCollectionCommand
        {
            get => (ICommand)GetValue(ItemCollectionCommandProperty);
            set => SetValue(ItemCollectionCommandProperty, value);
        }
    }
}
