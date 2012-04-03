using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace DeNSo
{
  [RunInstaller(true)]
  public partial class DeNSOInstaller : System.Configuration.Install.Installer
  {
    public DeNSOInstaller()
    {
      InitializeComponent();
    }
  }
}
