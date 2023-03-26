using Microsoft.EntityFrameworkCore;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Configuration;
using App.Services;
using HocAspMVC4_Test.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Cors.Infrastructure;


namespace DoAnCoSo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddDbContext<AppDbContext1>(options =>
        {
            string connectString = builder.Configuration.GetConnectionString("AppMvcConnectionString");
            options.UseSqlServer(connectString);
        });

        //cấu hình session
        builder.Services.AddDistributedMemoryCache();           // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
        builder.Services.AddSession(cfg => {                    // Đăng ký dịch vụ Session
            cfg.Cookie.Name = "appmvc";             // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
            cfg.IdleTimeout = new TimeSpan(0, 30, 0);    // Thời gian tồn tại của Session
        });


        //đăng ký identity
        builder.Services.AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext1>() //thiết lập làm việc vs csdl dựa trên dbcontext nào
        .AddDefaultTokenProviders();

        //cấu hình cho Identity   
        builder.Services.Configure<IdentityOptions>(options => {
            // Thiết lập về Password
            options.Password.RequireDigit = false; // Không bắt phải có số
            options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
            options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
            options.Password.RequireUppercase = false; // Không bắt buộc chữ in
            options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
            options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

            // Cấu hình Lockout - khóa user
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
            options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lần thì khóa
            options.Lockout.AllowedForNewUsers = true;

            // Cấu hình về User.
            options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;  // Email là duy nhất

            // Cấu hình đăng nhập.
            options.SignIn.RequireConfirmedEmail = false;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
            options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
            options.SignIn.RequireConfirmedAccount = false;
        });

        //cấu hình authorization theo policy
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AllowEditRole", policyBuilder =>
            {
                //điều kiện của policy
                policyBuilder.RequireAuthenticatedUser(); //user phải đăng nhập
                //policyBuilder.RequireRole("Admin"); //phải có role là Admin
                policyBuilder.RequireClaim("Manage", "add", "update");
            });

            options.AddPolicy("ShowAdminMenu", policyBuider =>
            {
                policyBuider.RequireRole("Admin");  //nếu user đó có role là admin thì có menu quản lý
            });

        });

        //cấu hình lỗi 
        builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/AccessDenied";
        });

        //builder.Services.AddAuthentication()
        //.AddGoogle(googleOptions =>
        //{
        //    // Đọc thông tin Authentication:Google từ appsettings.json
        //    IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");

        //    // Thiết lập ClientID và ClientSecret để truy cập API google
        //    googleOptions.ClientId = googleAuthNSection["606780764904-brch0njjvfarvtlvm4im15bqdgf2vctf.apps.googleusercontent.com"];
        //    googleOptions.ClientSecret = googleAuthNSection["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"];
        //    // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
        //    googleOptions.CallbackPath = "/dang-nhap-tu-google";

        //})
        //.AddFacebook(facebookOptions =>
        //{
        //    // Đọc cấu hình
        //    IConfigurationSection facebookAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");
        //    facebookOptions.AppId = facebookAuthNSection["534720661921442"];
        //    facebookOptions.AppSecret = facebookAuthNSection["7df0f6be87ebfe27cba466265c636793"];
        //    // Thiết lập đường dẫn Facebook chuyển hướng đến
        //    facebookOptions.CallbackPath = "/dang-nhap-tu-facebook";
        //});



        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();


        //app.UseStaticFiles(new StaticFileOptions()
        //{
        //    FileProvider = new PhysicalFileProvider(
        //        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
        //        ),
        //    RequestPath = "/contents"    // trong url ghi: content/1.jpg => mở file Uploads/1.jpg
        //});

        //dùng session
        app.UseSession();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

