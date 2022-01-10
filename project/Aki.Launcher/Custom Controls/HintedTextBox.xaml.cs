/* HintedTextBox.xaml.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * waffle.lord
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Aki.Launcher.Custom_Controls
{
    /// <summary>
    /// Interaction logic for HintedTextBox.xaml
    /// </summary>
    public partial class HintedTextBox : UserControl
    {
        public HintedTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register("HintColor", typeof(Brush), typeof(HintedTextBox), new PropertyMetadata(Brushes.Black));
        public Brush HintColor
        {
            get => (Brush)GetValue(HintColorProperty);
            set => SetValue(HintColorProperty, value);
        }

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(HintedTextBox), new PropertyMetadata(String.Empty));
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(string), typeof(HintedTextBox), new PropertyMetadata(String.Empty));
        public string Data
        {
            get => (string)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty ReturnKeyCommandProperty =
            DependencyProperty.Register("ReturnKeyCommand", typeof(ICommand), typeof(HintedTextBox), new PropertyMetadata(null));
        public ICommand ReturnKeyCommand
        {
            get => (ICommand)GetValue(ReturnKeyCommandProperty);
            set => SetValue(ReturnKeyCommandProperty, value);
        }

        public static readonly DependencyProperty ReturnKeyCommandParameterProperty =
            DependencyProperty.Register("ReturnKeyCommandParameter", typeof(object), typeof(HintedTextBox), new PropertyMetadata(null));
        public object ReturnKeyCommandParameter
        {
            get => (object)GetValue(ReturnKeyCommandParameterProperty);
            set => SetValue(ReturnKeyCommandParameterProperty, value);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Text.Length > 0)
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
