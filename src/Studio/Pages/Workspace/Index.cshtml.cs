using Microsoft.AspNetCore.Mvc.RazorPages;
using Studio.Entity;

namespace Studio.Pages.Workspace;

public class IndexModel : PageModel
{

    private readonly ContextBase _context;
    public int Id { get; set; }
    public Project Project { get; set; } = default!;


    public IndexModel(ContextBase context)
    {
        _context = context;
    }

    public async Task OnGet(int id)
    {
        Id = id;
        Project = await _context.Projects.FindAsync(id);

    }
}
