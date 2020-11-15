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
public class DeleteUserModel : PageModel
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly ApplicationDbContext _dbContext;

	public DeleteUserModel(
		UserManager<IdentityUser> userManager,
		ApplicationDbContext dbContext)
	{
		_userManager = userManager;
		_dbContext = dbContext;
	}

	[BindProperty]
	public IdentityUser IdentityUser{get; set;}

	public IActionResult OnGet(string id)
	{
		IdentityUser = _userManager.Users.FirstOrDefault(u => u.Id == id);
		if(IdentityUser == null){
			return RedirectToPage("./Index");
		}
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(string id)
	{
		IdentityUser = await _dbContext.Users.FindAsync(id);
		if(IdentityUser != null){
			await _userManager.DeleteAsync(IdentityUser);
			// await _signInManager.SignOutAsync();
		}

		return RedirectToPage("./Index");
	}
}
}