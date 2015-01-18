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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Company.VSPackage1.Forms
{
    /// <summary>
    /// Interaction logic for OpenProjectFileForm.xaml
    /// </summary>
    public partial class OpenProjectFileForm : Window
    {
        public OpenProjectFileForm()
        {
            InitializeComponent();
            MouseDown += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        this.DragMove();
                };
        }

        private IEnumerable<string> _values = null;
        public IEnumerable<string> ValuesInProjectList
        {
            get
            {
                return _values;
            }
            set
            {
                _values = value;
                lbItems.Items.Clear();
                foreach (var obj in _values)
                    lbItems.Items.Add(obj);
            }
        }
    }
}
