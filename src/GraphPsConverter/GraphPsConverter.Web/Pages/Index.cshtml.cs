using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GraphPsConverter.Core;

namespace GraphPsConverter.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public string SourceScript { get; set; }

        [BindProperty]
        public string ConvertedScript { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPostSubmit()
        {
            var converter = new Converter();
            var converted = converter.ConvertToGraphPowerShell(SourceScript);

            ConvertedScript = converted.ConvertedCommands[0].ConvertedScript;
        }

    }
}