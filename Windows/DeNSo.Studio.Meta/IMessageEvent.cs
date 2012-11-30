using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.Studio.Meta
{
  public interface IMessageEvent
  {
    void Handle(object message);
  }

  public interface IMessageEvent<T> : IMessageEvent
  {
    void Handle<T>(T message);
  }

  public interface ISimpleMessage
  {
    string MessageName { get; }
    void Handle(string message);
  }
}
