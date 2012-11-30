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
using DeNSo.Core;
using DeNSo.Studio.Meta.Entities;

namespace DeNSo.Studio.UserControls
{
  /// <summary>
  /// Interaction logic for DensoStoreConfigurationEditor.xaml
  /// </summary>
  public partial class DensoStoreConfigurationEditor : UserControl
  {
    public DensoStoreConfigurationEditor()
    {
      InitializeComponent();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      Session.New.Set<ConfiguredStore>(this.DataContext as ConfiguredStore);
    }
  }
}
