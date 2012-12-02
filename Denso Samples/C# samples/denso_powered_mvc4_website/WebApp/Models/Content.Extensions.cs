using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Models
{
  public static class ContentExtensions
  {
    public static MvcHtmlString GetImageUri(this Content item)
    {
      if (string.IsNullOrEmpty(item.Image)) return new MvcHtmlString(string.Empty);
      if (item.Image.StartsWith("http")) return new MvcHtmlString(item.Image);
      return new MvcHtmlString(string.Format("/Pages/GetImage/{0}", item.Image));
    }
  }
}