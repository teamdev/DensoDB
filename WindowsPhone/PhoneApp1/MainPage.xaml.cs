using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using PhoneApp1.Model;
using DeNSo;

namespace PhoneApp1
{
  public partial class MainPage : PhoneApplicationPage
  {
    // Constructor
    public MainPage()
    {
      InitializeComponent();

      var c = new Contact();
      c.Name = "Paolo";
      c.Sympathy = "very cool";

      var s = Session.New;
      s.Set(c).Wait();

      var result = s.Get<Contact>().ToList();
    }
  }
}