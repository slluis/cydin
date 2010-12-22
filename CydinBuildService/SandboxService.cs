// 
// SandboxService.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;

namespace CydinBuildService
{
	public static class SandboxService
	{
		static uint token;
		static bool inSandbox;
		static bool enabled;
		
		public static void Initialize ()
		{
			int res = 0;
			try {
				res = aa_change_profile ("cydin");
			} catch (DllNotFoundException) {
				throw new InvalidOperationException ("AppArmor initialization failed. Make sure AppArmor is properly installed.");
			}
			if (res != 0)
				throw new InvalidOperationException ("AppArmor profile could not be initialized");
			
			enabled = true;
		}
		
		public static void EnterSandbox ()
		{
			if (!inSandbox && enabled) {
				inSandbox = true;
				Random r = new Random ();
				token = ((uint) r.Next ());
				if (change_hat ("sandbox", token) != 0)
					throw new InvalidOperationException ("Sandbox could not be enabled");
			}
		}
		
		public static void ExitSandbox ()
		{
			if (inSandbox && enabled) {
				inSandbox = false;
				if (change_hat (null, token) != 0)
					throw new InvalidOperationException ("Sandbox could not be disabled");
			}
		}
	
		[DllImport("apparmor")]
		static extern int aa_change_profile(string profile);
	
		[DllImport("apparmor")]
		static extern int change_hat (string subprofile, uint magic_token);
	}
}

