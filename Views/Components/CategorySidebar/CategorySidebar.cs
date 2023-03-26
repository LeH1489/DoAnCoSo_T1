using System;
using Microsoft.AspNetCore.Mvc;
using Test123.Models;

namespace HocAspMVC4_Test.Views.Shared.Components.CategorySidebar
{
	[ViewComponent]
	public class CategorySidebar : ViewComponent
	{

		public class CategorySibarData
		{
			public List<Category> Categories { set; get; }

			public int level { set; get; }


			public string categoryslug { set; get; }
        }
		


		public IViewComponentResult Invoke(CategorySibarData data)
		{
			return View(data);
		}


	}
}

