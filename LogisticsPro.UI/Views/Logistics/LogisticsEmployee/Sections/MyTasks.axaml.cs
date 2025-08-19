using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LogisticsPro.UI.Views.Logistics.LogisticsEmployee.Sections
{
    public partial class MyTasks : UserControl
    {
        public MyTasks()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}