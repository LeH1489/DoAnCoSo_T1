using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace HocAspMVC4_Test.Menu
{
	public class AdminSidebarService
	{

		private readonly IUrlHelper UrlHelper;


		public AdminSidebarService(IUrlHelperFactory factory, IActionContextAccessor action)
		{
			UrlHelper = factory.GetUrlHelper(action.ActionContext);

			//khởi tạo các mục của sidebar
			Items.Add(new SidebarItem()
			{
				Type = SidebarItemType.Divider
			});
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.Heading,
				Title = "Quản lý chung"
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
				Controller = "Contact",
				Action = "Index",
				Area = "Contact",
				Title = "Quản lý liên hệ",
				AwesomeIcon = "fas fa-address-card"
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.Divider
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Role & Users",
                AwesomeIcon = "fas fa-user-cog",
				collapseID = "role",
				Items = new List<SidebarItem>()
				{
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
						Controller = "Role",
						Action = "Index",
						Area = "Identity",
						Title = "Các vai trò (Role)",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Role",
                        Action = "Create",
                        Area = "Identity",
                        Title = "Tạo Role mới",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "User",
                        Action = "Index",
                        Area = "Identity",
                        Title = "Danh sách user",
                    }
                }
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.Divider
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý bài viết",
                AwesomeIcon = "fas fa-user-cog",
                collapseID = "blog",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Category",
                        Action = "Index",
                        Area = "Blog",
                        Title = "Các danh mục",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Category",
                        Action = "Create",
                        Area = "Blog",
                        Title = "Tạo danh mục",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Post",
                        Action = "Index",
                        Area = "Blog",
                        Title = "Các bài viết",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "Post",
                        Action = "Create",
                        Area = "Blog",
                        Title = "Tạo bài viết",
                    }
                }
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.Divider
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý sản phẩm",
                AwesomeIcon = "fas fa-user-cog",
                collapseID = "product",
                Items = new List<SidebarItem>()
                {
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "CategoryProduct",
                        Action = "Index",
                        Area = "Product",
                        Title = "Các danh mục sản phẩm",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "CategoryProduct",
                        Action = "Create",
                        Area = "Product",
                        Title = "Tạo danh mục sản phẩm",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "ProductManage",
                        Action = "Index",
                        Area = "Product",
                        Title = "Các sản phẩm",
                    },
                    new SidebarItem() {
                        Type = SidebarItemType.NavItem,
                        Controller = "ProductManage",
                        Action = "Create",
                        Area = "Product",
                        Title = "Thêm sản phẩm",
                    }
                }
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.Divider
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "FileManager",
                Action = "Index",
                Area = "Files",
                Title = "Quản lý Files",
                AwesomeIcon = "fas fa-address-card"
            });



        }



        public List<SidebarItem> Items { set; get; } = new List<SidebarItem>();

		//trả về code html của tất cả p/tử trong Item
		public string renderHtml()
		{
			var html = new StringBuilder();
            foreach (var item in Items)
            {
				html.Append(item.RenderHtml(UrlHelper));
			}

			return html.ToString();
		}


		public void SetActive(string Controller, string Action, string Area)
		{
			foreach (var item in Items)
			{
				if (item.Controller  == Controller && item.Action == Action && item.Area == Area)
				{
					item.IsActive = true;
					return;
				}
				else
				{
					if (item.Items != null)   //phần tử con khác null
					{
						foreach (var childItem in item.Items)
						{
                            if (childItem.Controller == Controller && childItem.Action == Action && childItem.Area == Area)
							{
								childItem.IsActive = true;
								item.IsActive = true;
								return;
							}

                        }
					}
				}
			}
		}

		
	}
}

