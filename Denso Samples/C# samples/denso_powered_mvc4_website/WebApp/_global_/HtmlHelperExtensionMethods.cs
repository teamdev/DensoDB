using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;


public static class HtmlHelperExtensionMethods
{
  public static MvcHtmlString UploadFor<TModel, TProperty>
   (this HtmlHelper<TModel> helper,
   Expression<Func<TModel, TProperty>> expression, string id = null,
   string name = null, string cssclass = null, string alt = null,
   string imgId = null, string height = null, string width = null)
  {
    string fullHtmlFieldName = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText((expression)));

    if (id == null) id = fullHtmlFieldName;
    if (name == null) name = fullHtmlFieldName;

    var fileBuilder = new TagBuilder("input");
    fileBuilder.MergeAttribute("type", "File");
    fileBuilder.MergeAttribute("name", name);
    fileBuilder.MergeAttribute("id", id);

    //var required = helper.ViewContext.Controller.ValidateRequest;
    //if (required)
    //{
    //  fileBuilder.MergeAttribute("data-val-required", "*");
    //  fileBuilder.MergeAttribute("data-val", "true");
    //}

    return MvcHtmlString.Create(fileBuilder.ToString());
  }

  public static MvcHtmlString Upload
   (this HtmlHelper helper, string id = null,
   string name = null, string cssclass = null, string alt = null,
   string imgId = null, string height = null, string width = null, bool required = false)
  {
    var fileBuilder = new TagBuilder("input");
    fileBuilder.MergeAttribute("type", "File");
    fileBuilder.MergeAttribute("name", name);
    fileBuilder.MergeAttribute("id", id);

    //if (required)
    //{
    //  fileBuilder.MergeAttribute("data-val-required", "*");
    //  fileBuilder.MergeAttribute("data-val", "true");
    //}

    return MvcHtmlString.Create(fileBuilder.ToString());
  }
}
