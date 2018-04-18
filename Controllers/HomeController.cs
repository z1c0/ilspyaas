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

namespace ilspyaas.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "C# decompilation as a service.";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Decompile(IFormFile formFile, string typeName)
        {
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
                    var decompiler = GetDecompiler(filePath);
                    if (string.IsNullOrEmpty(typeName)) {
                        result.Code = decompiler.DecompileWholeModuleAsString();
                    } else {
                        var name = new FullTypeName(typeName);
                        result.Code = decompiler.DecompileTypeAsString(name);
                    }      
                    //File.Delete(filePath);          
                }
            }
            catch (Exception) {

            }
            return View(result);
        }

		static CSharpDecompiler GetDecompiler(string assemblyFileName)
		{
			return new CSharpDecompiler(assemblyFileName, new DecompilerSettings() {  ThrowOnAssemblyResolveErrors = false });
		}

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
