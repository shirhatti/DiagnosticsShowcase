using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiagnosticsShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;

        public ImageController(IWebHostEnvironment environment, ILoggerFactory loggerFactory)
        {
            _environment = environment;
            _logger = loggerFactory.CreateLogger<ImageController>();
        }

        [HttpGet("{fileName}")]
        public IActionResult Get(string fileName)
        {
            var filePath = Path.Join(_environment.ContentRootPath, "uploads", fileName);
            return PhysicalFile(filePath, "application/octet-stream");
        }

        [HttpPost]
        public async Task<IActionResult> UploadAsync(IFormFile file)
        {
            // full path to file in temp location
            var fileName = Guid.NewGuid().ToString();
            var filePath = Path.Join(_environment.ContentRootPath, "uploads", fileName);

            _logger.LogWarning(_environment.ContentRootPath);
            _logger.LogWarning(file.ContentType);

            if (file.Length > 0)
            {
                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    await file.CopyToAsync(stream);
                //}
                using (var stream = new MemoryStream())
                {
                    //await file.CopyToAsync(stream);
                    file.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    ConvertToPPMSlow(stream);
                }
                
            }

            return Ok();
        }

        private static void ConvertToPPMSlow(Stream stream)
        {
            if (System.IO.File.Exists("out.ppm"))
            {
                System.IO.File.Delete("out.ppm");
            }

            using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
            using (var outFile = System.IO.File.OpenWrite("out.ppm"))
            using (var writer = new BinaryWriter(outFile))
            {
                image.Mutate(x => x.Resize(250, 250));
                var outString = "";
                outString += "P3\n";
                outString += $"{image.Width} {image.Height}\n";
                outString += "255\n";
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        outString += $"{image[x, y].R} {image[x, y].G} {image[x, y].B} ";
                    }
                    outString += "\n";
                }

                var bytes = Encoding.UTF8.GetBytes(outString);
                writer.Write(bytes);
            }
        }

    }
}