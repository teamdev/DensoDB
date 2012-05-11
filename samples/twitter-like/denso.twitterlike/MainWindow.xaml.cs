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
using denso.twitterlike.model;
using DeNSo.Core;

namespace denso.twitterlike
{

  // This sample is not intended to show you WPF programming techniques, 
  // so will not be used any programming pattern like MVVM. 

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private ObjectDataProvider newmessageDataObject;
    private ObjectDataProvider messagelistDataObject;

    // Create a new densodb session
    private Session _denso = Session.New;

    public MainWindow()
    {
      InitializeComponent();

      PrepareBindingSources();
      PrepereMessageReceiving();
    }

    private void PrepereMessageReceiving()
    {
      Session.StoreUpdated += new EventHandler(Session_StoreUpdated);
    }

    void Session_StoreUpdated(object sender, EventArgs e)
    {
      messagelistDataObject.ObjectInstance = _denso.Get<Message>().ToList();
    }


    private void PrepareBindingSources()
    {
      // Get references to ObjectDataProvider used in the sample. 
      newmessageDataObject = this.Resources["newMessage"] as ObjectDataProvider;
      messagelistDataObject = this.Resources["MessageList"] as ObjectDataProvider;

      // I need to assign a DataObject Instance to my ObjectDataProvider, 
      // but before that i have to remove the ObjectType set in XAML definition.       
      newmessageDataObject.ObjectType = null;

      // Create a new Object to store new message
      newmessageDataObject.ObjectInstance = new Message();


      // Load the messagelist for the first time. 
      messagelistDataObject.ObjectType = null;
      messagelistDataObject.ObjectInstance = _denso.Get<Message>().ToList();
    }

    private void btnSendMessage(object sender, RoutedEventArgs e)
    {
      // send the message to densodb and to p2p mesh. 
      SendMessage();
    }

    private void SendMessage()
    {
      var msg = newmessageDataObject.Data as Message;
      if (msg != null)
      {
        msg.Date = DateTime.Now;
        _denso.Set<Message>(newmessageDataObject.Data as Message);
      }


      // prepare a new object message to store next message. 
      newmessageDataObject.ObjectInstance = new Message();
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      this.DragMove();
    }

    private void TextBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        SendMessage();
    }
  }
}
