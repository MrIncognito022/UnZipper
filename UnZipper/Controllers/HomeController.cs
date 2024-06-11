using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    // GET: Home/Index
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // POST: Home/Index
    [HttpPost]
    public async Task<IActionResult> Index(IFormFile zipFile)
    {
        // Logging to capture the request and state
        if (zipFile == null)
        {
            ViewBag.Message = "File is null";
            return View();
        }

        if (zipFile.Length == 0)
        {
            ViewBag.Message = "File is empty";
            return View();
        }

        ViewBag.Message = "File received: " + zipFile.FileName;

        string tempPath = Path.GetTempPath();
        string zipFilePath = Path.Combine(tempPath, zipFile.FileName);

        // Save the uploaded zip file to the temp directory
        using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
        {
            await zipFile.CopyToAsync(fileStream);
        }

        string extractedFilePath = null;

        // Extract .rpf file from the zip
        using (var archive = ZipFile.OpenRead(zipFilePath))
        {
            var rpfEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".rpf", System.StringComparison.OrdinalIgnoreCase));
            if (rpfEntry != null)
            {
                extractedFilePath = Path.Combine(tempPath, rpfEntry.Name);
                rpfEntry.ExtractToFile(extractedFilePath, true);
            }
        }

        // Clean up the zip file
        System.IO.File.Delete(zipFilePath);

        if (extractedFilePath == null)
        {
            ViewBag.Message = ".rpf file not found in the ZIP.";
            return View();
        }

        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(extractedFilePath);

        // Clean up the extracted file
        System.IO.File.Delete(extractedFilePath);

        return File(fileBytes, "application/octet-stream", Path.GetFileName(extractedFilePath));
    }
}
