using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySQLWeb.Data;

namespace MySQLWeb.Pages.Database
{
public class IndexModel : PageModel
{
	// private readonly RoleManager<IdentityRole> _roleManager;
	// private readonly UserManager<IdentityUser> _userManager;
	private readonly ApplicationDbContext _applicationDbContext;

	public IndexModel(
		// RoleManager<IdentityRole> roleManager,
		// UserManager<IdentityUser> userManager,
		ApplicationDbContext applicationDbContext)
	{
		// _roleManager = roleManager;
		// _userManager = userManager;
		_applicationDbContext = applicationDbContext;
	}

	public List<IdentityUser> Users{get; set;}
	public List<IdentityRole> Roles{get; set;}

	public IActionResult OnGet()
	{
		Users = _applicationDbContext.Users.ToList();
		Roles = _applicationDbContext.Roles.ToList();

		return Page();
	}
}
}