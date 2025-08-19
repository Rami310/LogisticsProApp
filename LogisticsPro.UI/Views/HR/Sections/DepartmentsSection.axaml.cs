using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LogisticsPro.UI.Views.HR.Sections
{
    public partial class DepartmentsSection : UserControl
    {
        public DepartmentsSection()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}