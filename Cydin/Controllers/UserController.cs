using Cydin.Properties;
using System.Security.Cryptography;
using System.Text;

namespace Cydin.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Cydin.Models;

	public class UserController : CydinController
	{
		private static OpenIdRelyingParty openid = new OpenIdRelyingParty ();

		public ActionResult Index ()
		{
			if (!User.Identity.IsAuthenticated) {
				Response.Redirect ("/User/Login?ReturnUrl=Index");
			}

			return View ("Index");
		}

		//public ActionResult LoginPopup ()
		//{
		//        return View ("LoginPopup");
		//}

		public ActionResult Logout ()
		{
			FormsAuthentication.SignOut ();
			return Redirect ("/Home");
		}

		public ActionResult Login ()
		{
			// Stage 1: display login form to user
			ViewData["ReturnUrl"] = "/Home";
			return View ("Login");
		}

		public ActionResult IdUpdateLogin (string ticket)
		{
			// Stage 1: display login form to user
			ViewData ["ticket"] = ticket;
			return View ("Login");
		}

		public ActionResult Profile (User u)
		{
			if (u.Login == null)
				u = CurrentUserModel.User;
			return View ("Profile", u);
		}

		[AcceptVerbs (HttpVerbs.Post)]
		public ActionResult ProfileSave (User user)
		{
			if (string.IsNullOrEmpty (user.Email))
				ModelState.AddModelError ("Email", "You must provide an email address");

			if (!ModelState.IsValid)
				return View ("Profile", user);
			
			User cuser = CurrentUserModel.GetUser (user.Id);
			cuser.Name = user.Name;
			cuser.Email = user.Email;
			CurrentUserModel.UpdateUser (cuser);
	
			return RedirectToAction ("Index", "Home");
		}

		[ValidateInput (false)]
		public ActionResult Authenticate (string returnUrl, string ticket)
		{
			var r = HttpContext.Request;
			bool updating = !string.IsNullOrEmpty (ticket);
			string loginView = "Login";
			var response = openid.GetResponse ();
			if (response == null) {
				// Stage 2: user submitting Identifier
				Identifier id;
				if (Identifier.TryParse (Request.Form["openid_identifier"], out id)) {
					try 
					{
						string host = updating ? Settings.Default.PreviousWebSiteHost : Settings.Default.WebSiteHost;
						Realm realm;
						if (host.All (c => char.IsDigit(c) || c=='.' || c==':'))
							realm = new Realm ("http://" + host);
						else
							realm = new Realm ("http://*." + host);
						
						IAuthenticationRequest req = openid.CreateRequest (Request.Form["openid_identifier"]);
//						IAuthenticationRequest req = openid.CreateRequest (Request.Form["openid_identifier"], realm);
						OutgoingWebResponse res = req.RedirectingResponse;
						return new InternalOutgoingWebResponseActionResult (res);
					}
					catch (ProtocolException ex) {
						ViewData["Message"] = ex.Message;
						return View (loginView);
					}
				}
				else {
					ViewData["Message"] = "Invalid identifier";
					return View (loginView);
				}
			}
			else {
				// Stage 3: OpenID Provider sending assertion response
				switch (response.Status) {
				case AuthenticationStatus.Authenticated:
					return AuthorizeUser (response.ClaimedIdentifier, ticket, returnUrl, null);
				case AuthenticationStatus.Canceled:
					ViewData["Message"] = "Canceled at provider";
					return View (loginView);
				case AuthenticationStatus.Failed:
					ViewData["Message"] = response.Exception.Message;
					return View (loginView);
				}
			}
			return new EmptyResult ();
		}

		public ActionResult GoogleAuthenticate (string returnUrl, string ticket)
		{
			var state = GetAuthState ();
			Session ["AuthReturnUrl"] = returnUrl;
			Session ["AuthTicket"] = ticket;
			var uri = UserModel.OAuth2AuthorizationRoot.Clients.First ().GetLoginLinkUri (state);
			return Redirect (uri);
		}

		string GetAuthState ()
		{
			MD5 md5 = MD5.Create ();
			byte[] hash = md5.ComputeHash (Encoding.UTF8.GetBytes (Session.SessionID));
			StringBuilder sb = new StringBuilder ();
			foreach (byte b in hash)
				sb.Append (b.ToString ("x"));
			return sb.ToString ();
		}

		public ActionResult OAuth2Authenticate ()
		{
			var ac = UserModel.OAuth2AuthorizationRoot.Clients.First ();
			var returnUrl = (string) Session ["AuthReturnUrl"];
			var ticket = (string) Session ["AuthTicket"];
			var state = Request.QueryString ["state"];
			var info = ac.GetUserInfo(Request.QueryString);
			if (state == GetAuthState ())
				return AuthorizeUser ("google-oauth2:" + info.Id, ticket, returnUrl, info.Email);
			ViewData["Message"] = "User could not be authenticated";
			return View ("Login");
		}

		ActionResult AuthorizeUser (string claimedUserId, string ticket, string returnUrl, string userEmail = null, string userName = null)
		{
			bool updating = !string.IsNullOrEmpty (ticket);

			User user = CurrentServiceModel.GetUserFromOpenId (claimedUserId);
			if (updating) {
				if (user == null) {
					ViewData["Message"] = "User not registered";
					return View ("Login");
				}
				string newId = GetTicketId (ticket);
				CurrentServiceModel.UpdateOpenId (claimedUserId, newId);
				FormsAuthentication.SignOut ();
			}

			// Try to migrate users using old Google OpenID to OAuth2
			if (user == null && !string.IsNullOrEmpty (userEmail)) {
				var currentUser = CurrentServiceModel.GetUserByEmail (userEmail);
				if (currentUser != null && currentUser.OpenId.StartsWith ("https://www.google.com/accounts/o8/id")) {
					CurrentServiceModel.UpdateOpenId (currentUser.OpenId, claimedUserId);
					user = currentUser;
				}
			}

			// This is a new user, send them to a registration page
			if (user == null) {
				ViewData["openid"] = claimedUserId;
				if (Settings.Default.SupportsMultiApps)
					return Redirect (string.Format ("~/home/User/register?openid={0}&name={1}&email={2}", Url.Encode (claimedUserId), Url.Encode (userName), Url.Encode (userEmail)));
				else
					return Redirect (string.Format ("~/User/register?openid={0}&name={1}&email={2}", Url.Encode (claimedUserId), Url.Encode (userName), Url.Encode (userEmail)));
			}

			FormsAuthentication.SetAuthCookie (user.Login, false);

			if (!string.IsNullOrEmpty (returnUrl)) 
				return Redirect (returnUrl);
			else if (updating)
				return Redirect (ControllerHelper.GetActionUrl ("home", "Index", "Home"));
			else
				return RedirectToAction ("Index", "Home");
		}

		public ActionResult Register (string openid, string name, string email)
		{
			User user = new User ();
			user.OpenId = openid;
			user.Name = name;
			user.Email = email;
			ViewData["ticket"] = GetIdTicket (user.OpenId);
			return View ("Registration", user);
		}

		[AcceptVerbs (HttpVerbs.Post)]
		public ActionResult Register (User user)
		{
			if (string.IsNullOrEmpty (user.Login) || string.IsNullOrEmpty (user.Email)) {
				if (string.IsNullOrEmpty (user.Login))
					ModelState.AddModelError ("Login", "You must pick a username");
				if (string.IsNullOrEmpty (user.Email))
					ModelState.AddModelError ("Email", "You must provide an email address");

				return View ("Registration", user);
			}

			if (CurrentServiceModel.IsUserNameAvailable (user.Login)) {
				CurrentServiceModel.CreateUser (user);

				FormsAuthentication.SetAuthCookie (user.Login, true);

				return RedirectToAction ("Index", "Home");
			}

			ModelState.AddModelError ("Name", "This username is not available, please choose another");

			return View ("Registration", user);
		}
		
		
		static Dictionary<string,string> openIdChangeTickets = new Dictionary<string, string> ();
		
		string GetIdTicket (string openId)
		{
			// Generates and registers a random ticket bound to the specified openId
			
			Random r = new Random ();
			byte[] buffer = new byte [40];
			string ticket;
			
			lock (openIdChangeTickets) {
				do {
					r.NextBytes (buffer);
					ticket = Convert.ToBase64String (buffer);
				} while (openIdChangeTickets.ContainsKey (ticket));
				
				openIdChangeTickets [ticket] = openId;
				return ticket;
			}
		}
		
		string GetTicketId (string ticket)
		{
			lock (openIdChangeTickets) {
				string id;
				if (openIdChangeTickets.TryGetValue (ticket, out id)) {
					openIdChangeTickets.Remove (ticket);
					return id;
				} else
					throw new Exception ("Invalid ticket");
			}
		}
	}
	
	internal class InternalOutgoingWebResponseActionResult : ActionResult {
		/// <summary>
		/// The outgoing web response to send when the ActionResult is executed.
		/// </summary>
		private readonly OutgoingWebResponse response;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutgoingWebResponseActionResult"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		internal InternalOutgoingWebResponseActionResult(OutgoingWebResponse response) {
			this.response = response;
		}

		/// <summary>
		/// Enables processing of the result of an action method by a custom type that inherits from <see cref="T:System.Web.Mvc.ActionResult"/>.
		/// </summary>
		/// <param name="context">The context in which to set the response.</param>
		public override void ExecuteResult(ControllerContext context) {
			this.response.Send();
		}
	}
}
