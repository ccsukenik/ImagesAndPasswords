using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ImagesAndPasswords.Data;
using ImagesAndPasswords.Web.Models;
using Newtonsoft.Json;

namespace ImagesAndPasswords.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=MyFirstDatabase; Integrated Security=true;";

        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile imageFile, Image image)
        {
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            using var fs = new FileStream(filePath, FileMode.CreateNew);
            imageFile.CopyTo(fs);
            image.FileName = fileName;
            var repo = new ImagesRepository(_connectionString);
            repo.AddImage(image);

            return View(image);
        }

        public IActionResult ViewImage(int id)
        {
            var vm = new ViewImageViewModel();
            if (TempData["message"] != null)
            {
                vm.Message = (string)TempData["message"];
            }

            if (!hasPermissionToView(id))
            {
                vm.HasPermissionToView = false;
                vm.Image = new Image { ID = id };
            }

            else
            {
                vm.HasPermissionToView = true;
                var repo = new ImagesRepository(_connectionString);
                repo.UpdateViews(id);
                var image = repo.GetImage(id);
                if (image == null)
                {
                    return RedirectToAction("index");
                }
                vm.Image = image;
            }

            return View(vm);
        }

        private bool hasPermissionToView(int id)
        {
            var allowedIDs = HttpContext.Session.Get<List<int>>("allowedIDs");
            if (allowedIDs == null)
            {
                return false;
            }
            return allowedIDs.Contains(id);
        }

       [HttpPost]
       public IActionResult ViewImageSubmit(int id, string password)
        {
            var repo = new ImagesRepository(_connectionString);
            var image = repo.GetImage(id);
            if (image == null)
            {
                return RedirectToAction("Index");
            }

            if (password != image.Password)
            {
                TempData["message"] = "Invalid password";
            }

            else
            {
                var allowedIDs = HttpContext.Session.Get<List<int>>("allowedIDs");
                if(allowedIDs == null)
                {
                    allowedIDs = new List<int>();
                }
                allowedIDs.Add(id);
                HttpContext.Session.Set("allowedIDs", allowedIDs);
            }

            return Redirect($"/home/viewimage?id={id}");
        }
    }
    public static class SessionExtensionMethods
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonConvert.DeserializeObject<T>(value);
        }
    }
}