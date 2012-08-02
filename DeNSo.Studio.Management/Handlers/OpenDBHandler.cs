using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeNSo.Studio.Management.Messages;
using DeNSo.Studio.Meta;

namespace DeNSo.Studio.Management.Handlers
{
  public class OpenDBHandler: IUIMessageHandler<OpenDB>
  {
    public void Handle<T>(T message) where T : class , IUIMessage
    {
      return Handle((OpenDB)message);
    }

    public void Handle(OpenDB message)
    {
      throw new NotImplementedException();
    }
  }
}
