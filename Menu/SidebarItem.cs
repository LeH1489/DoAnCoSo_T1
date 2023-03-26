using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace HocAspMVC4_Test.Menu
{
	public enum SidebarItemType
	{
		Divider,
		Heading,
		NavItem
	}


	public class SidebarItem
	{
		public string Title { set; get; }

		public bool IsActive { set; get; }

		public SidebarItemType Type { set; get; }

        public string Controller { set; get; }

        public string Action { set; get; }

        public string Area { set; get; }

        public string AwesomeIcon { set; get; } //lưu biểu tượng

		public List<SidebarItem>? Items { set; get;} //nav item có 2 loại: 1 - ko có menu con, 2 - có menu con

		public string collapseID { set; get; }

		public string GetLink(IUrlHelper urlHelper)
		{
			return urlHelper.Action(Action, Controller, new { area = Area });
		}


		public string RenderHtml(IUrlHelper urlHelper)
		{
			var html = new StringBuilder();

			if (Type == SidebarItemType.Divider)
			{
				html.Append("<hr class=\"sidebar-divider\">");
			}
			else if (Type == SidebarItemType.Heading)
			{
				html.Append(@$"
						<div class=""sidebar-heading"">
							{Title}
						</div>"
				);
			}
            else if (Type == SidebarItemType.NavItem)
            {
				if (Items == null) //ko có phần tử con
				{
					var url = GetLink(urlHelper); //đường link phát sinh
					var icon = (AwesomeIcon != null) ? $"<i class=\"{AwesomeIcon}\"></i>" : "";
					var cssClass = "nav-item";
					if (IsActive)
					{
						cssClass += " active";
					}


                    html.Append(@$"
						<li class=""{cssClass}"">
							<a class=""nav-link"" href=""{url}"">
							{icon}
							<span>{Title}</span>
							</a>
						</li>"
					);
				}
				else //có các phần tử con
				{
                    var icon = (AwesomeIcon != null) ? $"<i class=\"{AwesomeIcon}\"></i>" : "";
                    var cssClass = "nav-item";

                    if (IsActive)
                    {
                        cssClass += " active";
                    }

					var collapseCss = "collapse";
					if (IsActive)
					{
						collapseCss += " show";
                    }

					var itemMenu = "";

					foreach (var item in Items)
					{
						var urlItem = item.GetLink(urlHelper);
						var cssItem = "collapse-item";
						if (item.IsActive)
						{
							cssItem += " active";
						}
						itemMenu += $"<a class=\"{cssItem}\" href=\"{urlItem}\">{item.Title}</a>";
					}

                    html.Append(@$"
						 <li class=""{cssClass}"">
							<a class=""nav-link collapsed"" href=""#"" data-toggle=""collapse"" data-target=""#{collapseID}""
							   aria-expanded=""true"" aria-controls=""{collapseID}"">
								{icon}
								<span>{Title}</span>
							</a>
							<div id=""{collapseID}"" class=""{collapseCss}"" aria-labelledby=""headingTwo"" data-parent=""#accordionSidebar"">
								<div class=""bg-white py-2 collapse-inner rounded"">
									{itemMenu}
								</div>
							</div>
						</li>
					");

				}
            }

            return html.ToString();
		}

    }
}

