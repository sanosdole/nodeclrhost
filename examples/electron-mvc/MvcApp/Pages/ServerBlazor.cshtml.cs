namespace MvcApp.Pages
{
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    public class ServerBlazorModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public ServerBlazorModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        { }
    }
}
