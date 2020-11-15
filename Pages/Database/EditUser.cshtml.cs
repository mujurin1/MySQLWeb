using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySQLWeb.Data;

namespace MySQLWeb.Pages.Database
{
public class EditUserModel : PageModel
{
	private readonly ApplicationDbContext _dbContext;
	private readonly UserManager<IdentityUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;

	public EditUserModel(
		UserManager<IdentityUser> userManager,
		RoleManager<IdentityRole> roleManager,
		ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
		_userManager = userManager;
		_roleManager = roleManager;
	}

	[Required]
	[BindProperty]
	public InputUser EditUser{get; set;}
	[Required]
	[BindProperty]
	[Display(Name="ロール")]
	public List<RoleContext> Roles{get; set;}

	public class RoleContext{
		public IdentityRole Role{get; set;}
		public bool IsGet{get; set;}
	}

	public class InputUser {
		public string Id{get; set;}
		public string UserName{get; set;}
		public string Email{get; set;}
		public string PhoneNumber{get; set;}
	}

	public async Task<IActionResult> OnGet(string id)
	{
		var editUser = _userManager.Users.FirstOrDefault(m => m.Id == id);
		if(editUser == null){
			return RedirectToPage("./Index");
		}
		EditUser = new InputUser{
							Id = editUser.Id,
							UserName = editUser.UserName,
							Email = editUser.Email,
							PhoneNumber = editUser.PhoneNumber };

		var userRoles = (await _userManager.GetRolesAsync(editUser)).ToList();
		var allRoles = _roleManager.Roles;

		Roles = allRoles.Select(role => new RoleContext{
												Role = role,
												IsGet = userRoles.Contains(role.Name)})
						.ToList();

		return Page();
	}

	public async Task<IActionResult> OnPostAsync(params string[] roles)
	{
		if(!ModelState.IsValid){
			return Page();
		}
		var user = _userManager.Users.FirstOrDefault(m => m.Id == EditUser.Id);
		if(user == null){
			return Page();
		}

		user.UserName = EditUser.UserName;
		user.Email = EditUser.Email;
		user.PhoneNumber = EditUser.PhoneNumber;

		var result = await _userManager.UpdateAsync(user);
		if(!result.Succeeded){
			return Page();
		}
		// 新しいロール：１，２
		// 古いロール　：１，３
		// 全ロール　　：１，２，３，４
		// 
		// 古いロールと新しいロールを比べて、
		// 　古いロールにだけあるものを削除して、
		// 　新しいロールにだけあるものを追加する。
		// !!!!!!!!!!!!!!!!!!!!!!!今は全部消して全部追加してます!!!!!!!!!!!!!!!!!!!!!!!!
		var oldUserRole = await _userManager.GetRolesAsync(user);

		result = await _userManager.RemoveFromRolesAsync(user, oldUserRole);
		if(!result.Succeeded){
			return Page();
		}
		result = await _userManager.AddToRolesAsync(user, roles);
		if(!result.Succeeded){
			return Page();
		}

		return RedirectToPage("./Index");
	}
}
}
