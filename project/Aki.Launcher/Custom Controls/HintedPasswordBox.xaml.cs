/* HintedPasswordBox.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using Aki.Launcher.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Aki.Launcher.Custom_Controls
{
    /// <summary>
    /// Interaction logic for HintedPasswordBox.xaml
    /// </summary>
    public partial class HintedPasswordBox : UserControl, IHavePassword
    {
        public HintedPasswordBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register("HintColor", typeof(Brush), typeof(HintedPasswordBox), new PropertyMetadata(Brushes.Black));
        public Brush HintColor
        {
            get => (Brush)GetValue(HintColorProperty);
            set => SetValue(HintColorProperty, value);
        }

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(HintedPasswordBox), new PropertyMetadata(String.Empty));
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static readonly DependencyProperty ReturnKeyCommandProperty =
            DependencyProperty.Register("ReturnKeyCommand", typeof(ICommand), typeof(HintedPasswordBox), new PropertyMetadata(null));
        public ICommand ReturnKeyCommand
        {
            get => (ICommand)GetValue(ReturnKeyCommandProperty);
            set => SetValue(ReturnKeyCommandProperty, value);
        }

        public static readonly DependencyProperty ReturnKeyCommandParameterProperty =
            DependencyProperty.Register("ReturnKeyCommandParameter", typeof(object), typeof(HintedPasswordBox), new PropertyMetadata(null));
        public object ReturnKeyCommandParameter
        {
            get => (object)GetValue(ReturnKeyCommandParameterProperty);
            set => SetValue(ReturnKeyCommandParameterProperty, value);
        }

        public string Password => passBox.Password;

        private void passBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                if (pb.Password.Length > 0)
                {
                    HintLabel.Visibility = Visibility.Hidden;
                }
                else
                {
                    HintLabel.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
