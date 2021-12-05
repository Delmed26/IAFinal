using ImageAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using static ImageAPI.ImagesML;

namespace ImageAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult<ModelOutput> GetPredictions([FromBody]Byte[] ImageBytes)
        {

			Image image = null;
            String fullPath = String.Empty;
			using (var ms = new MemoryStream(ImageBytes))
			{
				image = Image.FromStream(ms);
			    String path = Path.GetTempPath();
			    String fileName = Guid.NewGuid().ToString() + ".jpg";
			    fullPath = Path.Combine(path, fileName);
			    image.Save(fullPath);
			}


			var imageModel = new ImagesML.ModelInput()
            {
                ImageSource = fullPath,
            };

            var result = ImagesML.Predict(imageModel);
            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}