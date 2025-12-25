using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Points.Windows.SignalCreation {
	/// <summary>
	/// Interaction logic for SignalCreationField.xaml
	/// </summary>
	[ObservableObject]
	[ContentProperty(nameof(InputControl))]
	public partial class SignalCreationFieldControl : UserControl {
		public SignalCreationFieldControl() {
			InitializeComponent();
		}

		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register(nameof(Caption), typeof(string), typeof(SignalCreationFieldControl), new PropertyMetadata(nameof(Caption)));

		public string Caption {
			get => (string)GetValue(CaptionProperty);
			set => SetValue(CaptionProperty, value);
		}

        public static readonly DependencyProperty InputControlProperty = 
            DependencyProperty.Register(nameof(InputControl), typeof(FrameworkElement), typeof(SignalCreationFieldControl), new PropertyMetadata(null));

        public FrameworkElement InputControl
        {
            get => (FrameworkElement)GetValue(InputControlProperty);
            set => SetValue(InputControlProperty, value);
        }
	}
}
