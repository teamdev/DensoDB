using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DeNSo.Studio.Meta;
using DeNSo.Studio.Meta.Entities;

namespace DeNSo.Studio.UserControls
{
  /// <summary>
  /// Interaction logic for ConfiguredStoreList.xaml
  /// </summary>
  public partial class ConfiguredStoreList : UserControl
  {
    public ConfiguredStoreList()
    {
      InitializeComponent();
    }

    private void btnNew_Click_1(object sender, RoutedEventArgs e)
    {
      DensoStoreConfigurationEditor editor = new DensoStoreConfigurationEditor();

      editor.DataContext = new ConfiguredStore() { Id = Guid.NewGuid().ToByteArray() };

      UIManager.ShowInRegion(editor, "centerArea");

    }
  }
}
