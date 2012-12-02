using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

using d = DeNSo;

namespace WebApp.Controllers
{
  public class PagesController : Controller
  {
    //
    // GET: /Pages/

    public ActionResult Show(string id, string layout = null)
    {

      var db = d.Session.New;
      var item = db.Get<Content>(i => i.Name == id).FirstOrDefault();

      if (!string.IsNullOrEmpty(layout))
        return View(layout, item);
      return View(item);
    }

    public ActionResult IndexCategory(string id, string layout = null)
    {
      var db = d.Session.New;
      var item = db.Get<Content>(i => i.Category == id).ToList();

      if (!string.IsNullOrEmpty(layout))
        return View(layout, item);
      return View(item);
    }

    public ActionResult Index()
    {
      var db = d.Session.New;
      return View(db.Get<Content>().ToList());
    }

    public ActionResult Create()
    {
      return View();
    }

    [HttpPost]
    public ActionResult Create(Content model)
    {
      if (ModelState.IsValid)
      {
        var db = d.Session.New;

        SaveImage(model);
        db.Set(model);

        return Redirect("Index");
      }
      return View(model);
    }

    public JsonResult Delete(int id)
    {
      var db = d.Session.New;

      var item = db.Get<Content>(i => i.Id == id).FirstOrDefault();
      if (item != null)
        try
        {
          db.Delete(item);
        }
        catch (Exception ex)
        {
          return Json(ex.Message, JsonRequestBehavior.AllowGet);
        }

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Edit(int id)
    {
      var db = d.Session.New;

      var item = db.Get<Content>(i => i.Id == id).FirstOrDefault();
      return View(item);
    }

    [HttpPost]
    public ActionResult Edit(int id, Content model)
    {
      if (ModelState.IsValid)
      {
        var db = d.Session.New;

        SaveImage(model);
        db.Set(model);

        return RedirectToAction("Index");
      }
      return View(model);

    }

    public ActionResult GetImage(string id)
    {
      var filename = FileManager.GetRealPath(id);
      if (System.IO.File.Exists(filename))
        return new FileContentResult(System.IO.File.ReadAllBytes(filename),
                                     "image/" + System.IO.Path.GetExtension(filename).Replace(".", ""));
      return new EmptyResult();
    }

    public JsonResult CheckName(string Name)
    {
      var db = d.Session.New;
      var cc = db.Get<Content>(c => c.Name == Name).Count();
      if (cc > 0)
      {
        return Json("Un contenuto con lo stesso nome già esiste", JsonRequestBehavior.AllowGet);
      }
      return Json(true, JsonRequestBehavior.AllowGet);
    }

    private static void SaveImage(Content model)
    {
      if (model.File != null && model.File.ContentLength > 0)
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          model.File.InputStream.CopyTo(memoryStream);
          model.Image = model.File.FileName;

          // ---------------------------------------------------
          // Salvo i dati con il mio HELPER 
          // ---------------------------------------------------
          model.Image = FileManager.SaveFile(model.File.FileName, memoryStream);
        }
      }
    }


  }
}
