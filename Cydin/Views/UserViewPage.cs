// 
// UserViewPage.cs
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
using Cydin.Models;
using Cydin.Controllers;

namespace Cydin.Views
{
	public class UserViewPage: System.Web.Mvc.ViewPage
	{
		UserModel model;
		ServiceModel serviceModel;
		
		public UserModel CurrentUserModel {
			get {
				if (model == null)
					model = UserModel.GetCurrent ();
				return model;
			}
		}
		
		public ServiceModel CurrentServiceModel {
			get {
				if (serviceModel == null)
					serviceModel = ServiceModel.GetCurrent ();
				return serviceModel;
			}
		}
		
		public string GetActionUrl (string action, string controller)
		{
			return ControllerHelper.GetActionUrl (CurrentUserModel.CurrentApplication.Name, action, controller);
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			if (model != null) {
				model.Dispose ();
				model = null;
			}
			if (serviceModel != null) {
				serviceModel.Dispose ();
				serviceModel = null;
			}
		}
	}

	public class UserViewPage<T>: System.Web.Mvc.ViewPage<T>
	{
		UserModel model;
		ServiceModel serviceModel;
		
		public UserModel CurrentUserModel {
			get {
				if (model == null)
					model = UserModel.GetCurrent ();
				return model;
			}
		}
		
		public ServiceModel CurrentServiceModel {
			get {
				if (serviceModel == null)
					serviceModel = ServiceModel.GetCurrent ();
				return serviceModel;
			}
		}
		
		public string GetActionUrl (string action, string controller)
		{
			return ControllerHelper.GetActionUrl (CurrentUserModel.CurrentApplication.Name, action, controller);
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			if (model != null) {
				model.Dispose ();
				model = null;
			}
			if (serviceModel != null) {
				serviceModel.Dispose ();
				serviceModel = null;
			}
		}
	}
}

