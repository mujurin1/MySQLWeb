using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySQLWeb.Data;

namespace MySQLWeb.Pages.Database
{
public class CreateRoleModel : PageModel
{
	private readonly ApplicationDbContext _applicationDbContext;
	private readonly RoleManager<IdentityRole> _roleManager;

	public CreateRoleModel(
		RoleManager<IdentityRole> roleManager,
		ApplicationDbContext applicationDbContext)
	{
		_applicationDbContext = applicationDbContext;
		_roleManager = roleManager;
	}

	[Required]
	[BindProperty]
	[Display(Name="ロール名")]
	[PageRemote(ErrorMessage ="Role Name already exists", AdditionalFields="__RequestVerificationToken",HttpMethod="Post",PageHandler="ExistsRole")]
	public string Role{get; set;}

	public IActionResult OnGet()
	{
		return Page();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<JsonResult> OnPostExistsRoleAsync()
	{
		if(await _roleManager.RoleExistsAsync(Role)){
			return new JsonResult($"ロール名「{Role}」は既に存在します");
		}
		return new JsonResult(true);
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if(!ModelState.IsValid){
			return Page();
		}
		var role = new IdentityRole(Role);
		var result = await _roleManager.CreateAsync(role);
		if(result.Succeeded){
			return RedirectToPage("./Index");
		}
		return Page();
	}
}
}