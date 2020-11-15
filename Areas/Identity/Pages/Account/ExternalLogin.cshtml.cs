using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace MySQLWeb.Areas.Identity.Pages.Account
{
[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly UserManager<IdentityUser> _userManager;
	private readonly IEmailSender _emailSender;
	private readonly ILogger<ExternalLoginModel> _logger;

	public ExternalLoginModel(
		SignInManager<IdentityUser> signInManager,
		UserManager<IdentityUser> userManager,
		ILogger<ExternalLoginModel> logger,
		IEmailSender emailSender)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_logger = logger;
		_emailSender = emailSender;
	}

	[BindProperty]
	public InputModel Input { get; set; }

	public string ProviderDisplayName { get; set; }

	public string ReturnUrl { get; set; }

	[TempData]
	public string ErrorMessage { get; set; }

	public class InputModel
	{
		[Required]
		public string UserName{get; set;}
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}

	public IActionResult OnGetAsync()
	{
		return RedirectToPage("./Login");
	}

	public IActionResult OnPost(string provider, string returnUrl = null)
	{
		// Request a redirect to the external login provider.
		var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
		var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
		Console.WriteLine(redirectUrl);
		return new ChallengeResult(provider, properties);
	}

	public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
	{
		returnUrl = returnUrl ?? Url.Content("~/");
		if (remoteError != null)
		{
			ErrorMessage = $"外部プロバイダーからのエラー：{remoteError}";
			return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
		}
		var info = await _signInManager.GetExternalLoginInfoAsync();
		if(info == null){
			ErrorMessage = "外部ログイン情報の読み込み中にエラーが発生しました";
			return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
		}

		// ユーザーがすでにログインしている場合は、この外部ログインプロバイダーを使用してユーザーにサインインします。
		var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
		if(result.Succeeded){
			_logger.LogInformation("{Name}は{LoginProvider}プロバイダーでログインしました。", info.Principal.Identity.Name, info.LoginProvider);
			return LocalRedirect(returnUrl);
		}
		if(result.IsLockedOut){
			return RedirectToPage("./Lockout");
		}else{
			// ユーザーがアカウントを持っていない場合は、ユーザーにアカウントの作成を依頼してください。
			ReturnUrl = returnUrl;
			ProviderDisplayName = info.ProviderDisplayName;
			if(info.Principal.HasClaim(c => c.Type == ClaimTypes.Email)){
				Input = new InputModel{
					UserName = info.Principal.FindFirstValue(ClaimTypes.Name),
					Email = info.Principal.FindFirstValue(ClaimTypes.Email)
				};
			}
			return Page();
		}
	}

	public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
	{
		returnUrl = returnUrl ?? Url.Content("~/");
		// Get the information about the user from the external login provider
		var info = await _signInManager.GetExternalLoginInfoAsync();
		if(info == null){
			ErrorMessage = "確認中に外部ログイン情報の読み込み中にエラーが発生しました";
			return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
		}

		if(ModelState.IsValid){
			var user = new IdentityUser { UserName = Input.UserName, Email = Input.Email };

			var result = await _userManager.CreateAsync(user);
			if(!result.Succeeded)
				foreach (var error in result.Errors)
					ModelState.AddModelError(string.Empty, error.Description);
			result = await _userManager.AddLoginAsync(user, info);
			if(result.Succeeded){
				_logger.LogInformation("ユーザーは{Name}プロバイダーを使用してアカウントを作成しました", info.LoginProvider);

				var userId = await _userManager.GetUserIdAsync(user);
				var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { area = "Identity", userId = userId, code = code },
					protocol: Request.Scheme);

				await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
					$"アカウントを確認してください <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>ここをクリック</a>.");

				// If account confirmation is required, we need to show the link if we don't have a real email sender
				if(_userManager.Options.SignIn.RequireConfirmedAccount){
					// return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
					// メールに確認コードを送る代わりに画面に表示してクリックしてもらうやつ
					// をここでしておく
					code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
					result = await _userManager.ConfirmEmailAsync(user, code);
					await _signInManager.SignInAsync(user, false);
					return LocalRedirect(returnUrl);
				}

				await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

				return LocalRedirect(returnUrl);
			}
		}

		ProviderDisplayName = info.ProviderDisplayName;
		ReturnUrl = returnUrl;
		return Page();
	}
}
}
