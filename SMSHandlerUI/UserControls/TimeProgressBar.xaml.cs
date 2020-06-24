using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SMSHandlerUI.UserControls
{
    public partial class TimeProgressBar : UserControl
    {
        #region Constructor

        public TimeProgressBar()
        {
            InitializeComponent();
        }

        #endregion

        #region Lower value definition

        public int ValueLower
        {
            get { return (int)GetValue(ValueLowerProperty); }
            set { SetValue(ValueLowerProperty, value); }
        }

        public static DependencyProperty ValueLowerProperty =
            DependencyProperty.Register("ValueLower", typeof(int), typeof(TimeProgressBar));

        #endregion

        #region Upper value definition

        public int ValueUpper
        {
            get { return (int)GetValue(ValueUpperProperty); }
            set { SetValue(ValueUpperProperty, value); }
        }

        public static DependencyProperty ValueUpperProperty =
            DependencyProperty.Register("ValueUpper", typeof(int), typeof(TimeProgressBar));


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
