using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Studio.Entity;
using Studio.Models;

namespace Studio.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ContextBase _context;

    public List<Project> Projects { get; set; } = new List<Project>();

    [BindProperty]
    public AddProjectForm ProjectForm { get; set; }

    public IndexModel(ILogger<IndexModel> logger, ContextBase context)
    {
        _logger = logger;
        _context = context;
        ProjectForm = new AddProjectForm() { DisplayName = "", Path = "" };
    }

    public async Task OnGetAsync()
    {
        //  查询项目列表
        Projects = await _context.Projects.ToListAsync();
    }

    public async Task<ActionResult> OnPostAsync()
    {
        Project project = new()
        {
            DisplayName = ProjectForm.DisplayName,
            Path = ProjectForm.Path,
            Name = ""
        };

        // TODO:获取其他相关信息

        _ = await _context.Projects.AddAsync(project);
        _ = await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}
