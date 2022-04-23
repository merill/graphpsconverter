using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GraphPsConverter.Core;
using GraphPsConverter.Core.Model;

namespace GraphPsConverter.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public string SourceScript { get; set; }

        [BindProperty]
        public ParsedScript ParsedScript { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPostSubmit()
        {
            _logger.LogInformation(SourceScript);
            var converter = new Converter();
            ParsedScript = converter.ConvertToGraphPowerShell(SourceScript);
        }

    }
}