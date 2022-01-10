/* EditProfileView.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * Merijn Hendriks
 */


using Aki.Launcher.Interfaces;
using System.Windows.Controls;

namespace Aki.Launcher.Views
{
    /// <summary>
    /// Interaction logic for EditProfileView.xaml
    /// </summary>
    public partial class EditProfileView : UserControl, IHavePassword
    {
        public EditProfileView()
        {
            InitializeComponent();
        }

        public string Password => passBox.Password;
    }
}
