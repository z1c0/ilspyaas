using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ilspyaas.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;

namespace ilspyaas.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View("Upload");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "C# decompilation as a service.";

            return View();
        }

        [HttpGet]
        public IActionResult Decompile(string wellknown)
        {
            var result = new ResultViewModel();
            var path = Path.Combine(_hostingEnvironment.WebRootPath, string.Format($"data/{wellknown}.dll"));
            if (System.IO.File.Exists(path))
            {
                result.Code = DecompileAssembly(path);
            }
            else 
            {
                result.Error = "Invalid operation";
            }
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Decompile(IFormFile formFile)
        {
            // TODO: logging
            // TODO: error page
            var result = new ResultViewModel();
            try
            {
                // full path to file in temp location
                var filePath = Path.GetTempFileName();
                if (formFile != null && formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    result.Code = DecompileAssembly(filePath);
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception e) {
                result.Error = e.ToString();
            }
            return View(result);
        }

        private static string DecompileAssembly(string filePath)
        {
            var code = string.Empty;
            var decompiler = GetDecompiler(filePath);
            var typeName = string.Empty;
            if (string.IsNullOrEmpty(typeName))
            {
                code = decompiler.DecompileWholeModuleAsString();
            }
            else
            {
                var name = new FullTypeName(typeName);
                code = decompiler.DecompileTypeAsString(name);
            }
            return code;
        }

        static CSharpDecompiler GetDecompiler(string assemblyFileName)
		{
			return new CSharpDecompiler(assemblyFileName, new DecompilerSettings() 
            {
                ThrowOnAssemblyResolveErrors = false,
                LoadInMemory = true
            });
		}

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
