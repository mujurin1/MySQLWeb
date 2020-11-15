using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySQLWeb.Data;

namespace MySQLWeb.Pages.Database
{
public class DeleteRoleModel : PageModel
{
	private readonly ApplicationDbContext _dbContext;

	public DeleteRoleModel(
		ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	[BindProperty]
	public IdentityRole Role{get; set;}

	public IActionResult OnGet(string id)
	{
		if(id == null){
			return RedirectToPage("./Index");
		}
		Role = _dbContext.Roles.FirstOrDefault(m => m.Id == id);
		if(Role == null){
			return RedirectToPage("./Index");
		}
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(string id)
	{
		if(id == null){
			return RedirectToPage("./Index");
		}
		Role = _dbContext.Roles.FirstOrDefault(m => m.Id == id);
		if(Role == null){
			return RedirectToPage("./Index");
		}
		_dbContext.Roles.Remove(Role);
		await _dbContext.SaveChangesAsync();

		return RedirectToPage("./Index");
	}
}
}