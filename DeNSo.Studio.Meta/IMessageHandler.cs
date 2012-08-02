using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Studio.Meta
{
  public interface IUIMessageHandler
  {
    void Handle<T>(T message) where T : IUIMessage;
  }

  public interface IUIMessageHandler<T> : IUIMessageHandler where T :IUIMessage
  {
    void Handle(T message);
  }

  public interface IUIMessage
  { }
}
