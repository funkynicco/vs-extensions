using Company.VSPackage1.Classes;
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
        private readonly IEnumerable<FileListObject> _allObjects;

        public FileListObject SelectedListObject { get; private set; }

        public OpenProjectFileForm(IEnumerable<FileListObject> allObjects)
        {
            _allObjects = allObjects;

            InitializeComponent();
            SelectedListObject = null;
            MouseDown += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        this.DragMove();
                };

            txtSearch.Focus();

            txtSearch.TextChanged += (sender, e) => UpdateVisibleObjects();

            UpdateVisibleObjects();
        }

        void UpdateVisibleObjects()
        {
            var search = txtSearch.Text.ToLower();

            lbItems.Items.Clear();
            foreach (var obj in _allObjects)
            {
                if (obj.Name.ToLower().Contains(search))
                    lbItems.Items.Add(obj);
            }

            if (lbItems.Items.Count > 0)
                lbItems.SelectedIndex = 0;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }

            bool internal_handled = false;

            if (lbItems.Items.Count > 0)
            {
                if (e.Key == Key.Down)
                {
                    if (lbItems.SelectedIndex == -1)
                        lbItems.SelectedIndex = 0;
                    else
                        lbItems.SelectedIndex = (lbItems.SelectedIndex + 1) % lbItems.Items.Count;
                    internal_handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    if (lbItems.SelectedIndex != -1)
                    {
                        int index = lbItems.SelectedIndex - 1;
                        if (index < 0)
                            index = lbItems.Items.Count - 1;
                        lbItems.SelectedIndex = index;
                    }
                    else
                        lbItems.SelectedIndex = lbItems.Items.Count - 1;
                    internal_handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    if (lbItems.SelectedIndex != -1)
                    {
                        SelectedListObject = (FileListObject)lbItems.SelectedItem;

                        e.Handled = true;
                        Close();
                        return;
                    }
                }
            }

            if (internal_handled)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            lbItems.SelectedIndex = lbItems.Items.Count > 0 ? 0 : -1;

            base.OnPreviewKeyDown(e);
            txtSearch.Focus();
        }
    }
}
