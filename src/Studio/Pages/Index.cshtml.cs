using Microsoft.AspNetCore.Mvc.RazorPages;
using Studio.Entity;

namespace Studio.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ContextBase _context;

    public List<Project> Projects { get; set; } = new List<Project>();

    public IndexModel(ILogger<IndexModel> logger, ContextBase context)
    {
        _logger = logger;
        _context = context;
    }

    public void OnGet()
    {
        //  查询项目列表

    }
}
