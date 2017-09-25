using LibrarySite.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Security.Claims;

[assembly: OwinStartupAttribute(typeof(LibrarySite.Startup))]
namespace LibrarySite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers(); 
        }

        // In this method we will create default User roles and Admin user for login   
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<CustomRole, int>(new RoleStore<CustomRole, int, CustomUserRole>(context));
   

            var UserManager = new UserManager<ApplicationUser, int>(new UserStore<ApplicationUser, CustomRole,
                int, CustomUserLogin, CustomUserRole, CustomUserClaim>(context)); //(new UserStore<ApplicationUser>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin role   
                //var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole<int, CustomUserRole>();
                //role.Name = "Admin";
                //RoleManagerExtensions.Create<CustomRole, int>(roleManager, role);
                var role = new CustomRole("Admin");
                context.Roles.Add(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "supwn";
                user.Email = "perrycn@charter.net";

                string userPWD = "A@Z200711";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }

            // creating Creating Manager role    
            if (!roleManager.RoleExists("Librarian"))
            {
                //var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole<int, CustomUserRole>();
                //role.Name = "Librarian";
                var role = new CustomRole("Librarian");
                RoleManagerExtensions.Create((RoleManager<CustomRole, int>)roleManager, (CustomRole)role);
                //context.Roles.Add(role);
                //context.SaveChanges();
            }

            // creating Creating Employee role    
            if (!roleManager.RoleExists("Member"))
            {
                var role = new CustomRole("Member");
                RoleManagerExtensions.Create((RoleManager<CustomRole, int>)roleManager, (CustomRole)role);
                //context.Roles.Add(role);
            }
        } 
    }
}
