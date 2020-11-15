using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace MySQLWeb.Areas.Identity.Pages.Account
{
[AllowAnonymous]
public class ConfirmEmailModel : PageModel
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly ILogger<LoginModel> _logger;

	public ConfirmEmailModel(
		SignInManager<IdentityUser> signInManager,
		ILogger<LoginModel> logger,
		UserManager<IdentityUser> userManager)
	{
		_signInManager = signInManager;
		_logger = logger;
		_userManager = userManager;
	}

	[TempData]
	public string StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync(string userId, string code)
	{
		if(userId == null || code == null){
			return RedirectToPage("/Index");
		}

		var user = await _userManager.FindByIdAsync(userId);
		if(user == null){
			return NotFound($"Unable to load user with ID '{userId}'.");
		}

		code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		var result = await _userManager.ConfirmEmailAsync(user, code);
		if(result.Succeeded){
			_logger.LogInformation("User logged in.");
			StatusMessage = "メールをご確認いただきありがとうございます";
			await _signInManager.SignInAsync(user, false);
			return RedirectToPage();
		}else{
			StatusMessage = "メールの確認中にエラーが発生しました";
			return Page();
		}
	}
}
}
