using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MySQLWeb.Areas.Identity.Pages.Account.Manage
{
public partial class IndexModel : PageModel
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly SignInManager<IdentityUser> _signInManager;

	public IndexModel(
		UserManager<IdentityUser> userManager,
		SignInManager<IdentityUser> signInManager)
	{
		_userManager = userManager;
		_signInManager = signInManager;
	}

	[Display(Name="User Name")]
	public string Username { get; set; }

	[TempData]
	public string StatusMessage { get; set; }

	[BindProperty]
	public InputModel Input { get; set; }

	public class InputModel
	{
		[Phone]
		[Display(Name = "Phone number")]
		public string PhoneNumber { get; set; }
		[Display(Name="User Name")]
		public string UserName{get; set;}
	}

	private async Task LoadAsync(IdentityUser user)
	{
		var userName = await _userManager.GetUserNameAsync(user);
		var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
		Username = userName;
		Input = new InputModel{
			UserName = userName,
			PhoneNumber = phoneNumber
		};
	}

	public async Task<IActionResult> OnGetAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if(user == null){
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		await LoadAsync(user);
		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if(user == null){
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		if(!ModelState.IsValid){
			await LoadAsync(user);
			return Page();
		}

		var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
		if(Input.PhoneNumber != phoneNumber){
			var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
			if(!setPhoneResult.Succeeded){
				StatusMessage = "Unexpected error when trying to set phone number.";
				return RedirectToPage();
			}
		}
		var userName = await _userManager.GetUserNameAsync(user);
		if(Input.UserName != userName){
			var setNameResult = await _userManager.SetUserNameAsync(user, Input.UserName);
			if(!setNameResult.Succeeded){
				StatusMessage = "Unexpected error when trying to set user name.";
				return RedirectToPage();
			}
		}

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "Your profile has been updated";
		return RedirectToPage();
	}
}
}
