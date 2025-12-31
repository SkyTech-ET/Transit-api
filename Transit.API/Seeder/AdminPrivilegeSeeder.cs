using Microsoft.AspNetCore.Mvc.Infrastructure;
using Transit.Domain.Data;
using Transit.Domain.Models;

namespace Transit.API;


public static class AdminPrivilegeSeeder
{
    public static void Seed(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var actionDescriptorProvider = scope.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();

        // 1. Seed privileges
        var actions = actionDescriptorProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(a => !a.ControllerName.Contains("Health")) // optional
            .ToList();

        foreach (var action in actions)
        {
            var actionKey = $"{action.ControllerName}-{action.ActionName}";

            if (!db.Privilege.Any(p => p.Action == actionKey))
            {
                db.Privilege.Add(new Privilege
                {
                    Action = actionKey,
                    Description = $"Access {action.ControllerName}.{action.ActionName}"
                });
            }
        }

        db.SaveChanges();

        // 2. Ensure SuperAdmin role exists
        var superAdminRole = db.Role.FirstOrDefault(r => r.Name == "SuperAdmin");

        if (superAdminRole == null)
        {
            superAdminRole = Role.Create("SuperAdmin", "Super administrator with all privileges");
            db.Role.Add(superAdminRole);
            db.SaveChanges();
        }

        // 3. Assign all privileges to SuperAdmin
        var allPrivileges = db.Privilege.ToList();

        foreach (var privilege in allPrivileges)
        {
            bool exists = db.RolePrivilege.Any(rp =>
                rp.RoleId == superAdminRole.Id &&
                rp.PrivilegeId == privilege.Id);

            if (!exists)
            {
                var rolePrivilege = new RolePrivilege
                {
                    RoleId = superAdminRole.Id,
                    PrivilegeId = privilege.Id
                };
                superAdminRole.AddRolePrivilege(rolePrivilege);
                db.RolePrivilege.Add(rolePrivilege);
            }
        }

        db.SaveChanges();
    }

}
