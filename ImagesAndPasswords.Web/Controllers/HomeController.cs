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

        private readonly string ImageIdsSessionName = "ImageIdsAndPswd";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile imageFile, string password)
        {
            string fileName = $"{Guid.NewGuid()}-{imageFile.FileName}";

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            using var fs = new FileStream(filePath, FileMode.CreateNew);
            imageFile.CopyTo(fs);

            var repo = new ImagesRepository(_connectionString);
            var id = repo.AddImage(fileName, password);

            var vm = new UploadViewModel
            {
                ID = id,
                Password = password
            };

            return View(vm);
        }

        public IActionResult EnterPassword(int id)
        {
            var imageIds = HttpContext.Session.Get<List<int>>(ImageIdsSessionName);
            if (imageIds != null && imageIds.Contains(id))
            {
                return Redirect($"/home/viewimage?id={id}");
            }

            var vm = new PasswordViewModel { ID = id, InvalidPassword = (string)TempData["InvalidPassword"] };
            return View(vm);
        }

        public IActionResult ViewImage(int id)
        {
            var imageIds = HttpContext.Session.Get<List<int>>(ImageIdsSessionName);
            if(imageIds == null || !imageIds.Contains(id))
            {
                return Redirect($"home/enterpassword?id={id}");
            }
            var repo = new ImagesRepository(_connectionString);
            var image = repo.GetImage(id);
            repo.UpdateView(id);
            var vm = new ImageViewModel { Image = image };
            return View(vm);
        }

        public IActionResult AddImage(int id, string password)
        {
            var repo = new ImagesRepository(_connectionString);
            string invalidPassword = repo.GetPassword(id);
            var imageIds = HttpContext.Session.Get<List<int>>(ImageIdsSessionName);
            if (imageIds == null)
            {
                imageIds = new List<int>();
            }

            if (password == invalidPassword)
            {
                imageIds.Add(id);
                HttpContext.Session.Set(ImageIdsSessionName, imageIds);
                return Redirect($"/home/viewimage?id={id}");
            }

            else
            {
                TempData["invalidPassword"] = "Invalid Password! Please try again!";
                return Redirect($"/home/enterpassword?id={id}");
            }
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