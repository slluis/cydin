using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Security.Cryptography;

namespace Cydin.Models
{
	public class User
	{
		[DataMember (Identity=true)]
		public int Id { get; set; }

		[DataMember] public string Login { get; set; }
		[DataMember] public string Name { get; set; }
		[DataMember] public bool IsAdmin { get; set; }
		[DataMember] public string Email { get; set; }
		[DataMember] public string OpenId { get; set; }
		[DataMember] public SiteNotification SiteNotifications { get; set; }
		[DataMember ("Password")] public string PasswordHash { get; set; }
		[DataMember] public string PasswordSalt { get; set; }
		
		public void SetPassword (string password)
		{
			// Generate a random salt
			Random r = new Random ();
			byte[] buffer = new byte [40];
			r.NextBytes (buffer);
			PasswordSalt = Convert.ToBase64String (buffer);
			
			SHA256 shaM = new SHA256Managed(); 
			byte[] result = shaM.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password + PasswordSalt));
			PasswordHash = Convert.ToBase64String (result);
		}
		
		public bool CheckPassword (string password)
		{
			SHA256 shaM = new SHA256Managed(); 
			byte[] result = shaM.ComputeHash (System.Text.Encoding.UTF8.GetBytes (password + PasswordSalt));
			return PasswordHash == Convert.ToBase64String (result);
		}
	}
}