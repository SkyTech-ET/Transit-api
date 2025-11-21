using Microsoft.EntityFrameworkCore;
using Transit.Domain.Data;
using Transit.Domain.Models.Shared;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Transit.API.TestScripts;

public class EndToEndTest
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;

    public EndToEndTest(ApplicationDbContext context, PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<TestResults> RunCompleteWorkflowTest()
    {
        var results = new TestResults();
        
        Console.WriteLine("🚀 Starting Complete End-to-End Workflow Test");
        Console.WriteLine(new string('=', 60));

        try
        {
            // Step 1: Test Data Seeding
            Console.WriteLine("\n📊 Step 1: Testing Data Seeding...");
            results.DataSeeding = await TestDataSeeding();
            Console.WriteLine($"✅ Data Seeding: {(results.DataSeeding ? "PASSED" : "FAILED")}");

            // Step 2: Test SuperAdmin Flow
            Console.WriteLine("\n👑 Step 2: Testing SuperAdmin Flow...");
            results.SuperAdminFlow = await TestSuperAdminFlow();
            Console.WriteLine($"✅ SuperAdmin Flow: {(results.SuperAdminFlow ? "PASSED" : "FAILED")}");

            // Step 3: Test DataEncoder Flow (Customer Creation)
            Console.WriteLine("\n📝 Step 3: Testing DataEncoder Flow (Customer Creation)...");
            results.DataEncoderFlow = await TestDataEncoderFlow();
            Console.WriteLine($"✅ DataEncoder Flow: {(results.DataEncoderFlow ? "PASSED" : "FAILED")}");

            // Step 4: Test Assessor Flow (Customer Approval)
            Console.WriteLine("\n🔍 Step 4: Testing Assessor Flow (Customer Approval)...");
            results.AssessorFlow = await TestAssessorFlow();
            Console.WriteLine($"✅ Assessor Flow: {(results.AssessorFlow ? "PASSED" : "FAILED")}");

            // Step 5: Test Customer Flow (Service Request)
            Console.WriteLine("\n👤 Step 5: Testing Customer Flow (Service Request)...");
            results.CustomerFlow = await TestCustomerFlow();
            Console.WriteLine($"✅ Customer Flow: {(results.CustomerFlow ? "PASSED" : "FAILED")}");

            // Step 6: Test Manager Flow (Service Assignment)
            Console.WriteLine("\n👔 Step 6: Testing Manager Flow (Service Assignment)...");
            results.ManagerFlow = await TestManagerFlow();
            Console.WriteLine($"✅ Manager Flow: {(results.ManagerFlow ? "PASSED" : "FAILED")}");

            // Step 7: Test CaseExecutor Flow (Service Execution)
            Console.WriteLine("\n⚡ Step 7: Testing CaseExecutor Flow (Service Execution)...");
            results.CaseExecutorFlow = await TestCaseExecutorFlow();
            Console.WriteLine($"✅ CaseExecutor Flow: {(results.CaseExecutorFlow ? "PASSED" : "FAILED")}");

            // Step 8: Test Complete Workflow
            Console.WriteLine("\n🔄 Step 8: Testing Complete End-to-End Workflow...");
            results.CompleteWorkflow = await TestCompleteWorkflow();
            Console.WriteLine($"✅ Complete Workflow: {(results.CompleteWorkflow ? "PASSED" : "FAILED")}");

            // Step 9: Test Document Management
            Console.WriteLine("\n📄 Step 9: Testing Document Management...");
            results.DocumentManagement = await TestDocumentManagement();
            Console.WriteLine($"✅ Document Management: {(results.DocumentManagement ? "PASSED" : "FAILED")}");

            // Step 10: Test Messaging System
            Console.WriteLine("\n💬 Step 10: Testing Messaging System...");
            results.MessagingSystem = await TestMessagingSystem();
            Console.WriteLine($"✅ Messaging System: {(results.MessagingSystem ? "PASSED" : "FAILED")}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed with exception: {ex.Message}");
            results.OverallSuccess = false;
        }

        results.OverallSuccess = results.DataSeeding && results.SuperAdminFlow && results.DataEncoderFlow && 
                                results.AssessorFlow && results.CustomerFlow && results.ManagerFlow && 
                                results.CaseExecutorFlow && results.CompleteWorkflow && results.DocumentManagement && 
                                results.MessagingSystem;

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine($"🎯 OVERALL TEST RESULT: {(results.OverallSuccess ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Console.WriteLine(new string('=', 60));

        return results;
    }

    private async Task<bool> TestDataSeeding()
    {
        try
        {
            // Check if all roles exist
            var roles = await _context.Role.ToListAsync();
            var expectedRoles = new[] { "SuperAdmin", "Manager", "Assessor", "CaseExecutor", "DataEncoder", "Customer" };
            
            foreach (var expectedRole in expectedRoles)
            {
                if (!roles.Any(r => r.Name == expectedRole))
                {
                    Console.WriteLine($"❌ Missing role: {expectedRole}");
                    return false;
                }
            }

            // Check if all users exist
            var users = await _context.Users.ToListAsync();
            var expectedUsers = new[] { "superadmin", "manager", "assessor", "caseexecutor", "dataencoder", "customer" };
            
            foreach (var expectedUser in expectedUsers)
            {
                if (!users.Any(u => u.Username == expectedUser))
                {
                    Console.WriteLine($"❌ Missing user: {expectedUser}");
                    return false;
                }
            }

            // Check if privileges are assigned
            var rolePrivileges = await _context.RolePrivilege.CountAsync();
            if (rolePrivileges == 0)
            {
                Console.WriteLine("❌ No role privileges assigned");
                return false;
            }

            // Check if user roles are assigned
            var userRoles = await _context.UserRole.CountAsync();
            if (userRoles == 0)
            {
                Console.WriteLine("❌ No user roles assigned");
                return false;
            }

            Console.WriteLine($"✅ Found {roles.Count} roles, {users.Count} users, {rolePrivileges} role privileges, {userRoles} user roles");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Data seeding test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestSuperAdminFlow()
    {
        try
        {
            var superAdmin = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "superadmin");

            if (superAdmin == null)
            {
                Console.WriteLine("❌ SuperAdmin user not found");
                return false;
            }

            // Test authentication
            var token = GenerateJwtToken(superAdmin);
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("❌ Failed to generate JWT token for SuperAdmin");
                return false;
            }

            // Test role verification
            var hasSuperAdminRole = superAdmin.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin");
            if (!hasSuperAdminRole)
            {
                Console.WriteLine("❌ SuperAdmin user doesn't have SuperAdmin role");
                return false;
            }

            // Test password verification
            var passwordValid = _passwordService.ValidatePassword("Admin123!", superAdmin.Password);
            if (!passwordValid)
            {
                Console.WriteLine("❌ SuperAdmin password verification failed");
                return false;
            }

            Console.WriteLine($"✅ SuperAdmin authentication and role verification successful");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SuperAdmin flow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestDataEncoderFlow()
    {
        try
        {
            var dataEncoder = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "dataencoder");

            if (dataEncoder == null)
            {
                Console.WriteLine("❌ DataEncoder user not found");
                return false;
            }

            // Test customer creation capability
            var hasCustomerCreatePrivilege = await HasPrivilege(dataEncoder.Id, "Customer-Create");
            if (!hasCustomerCreatePrivilege)
            {
                Console.WriteLine("❌ DataEncoder doesn't have Customer-Create privilege");
                return false;
            }

            // Test service creation capability
            var hasServiceCreatePrivilege = await HasPrivilege(dataEncoder.Id, "Service-Create");
            if (!hasServiceCreatePrivilege)
            {
                Console.WriteLine("❌ DataEncoder doesn't have Service-Create privilege");
                return false;
            }

            Console.WriteLine($"✅ DataEncoder has required privileges for customer and service creation");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DataEncoder flow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestAssessorFlow()
    {
        try
        {
            var assessor = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "assessor");

            if (assessor == null)
            {
                Console.WriteLine("❌ Assessor user not found");
                return false;
            }

            // Test customer approval capability
            var hasCustomerApprovePrivilege = await HasPrivilege(assessor.Id, "Customer-Approve");
            if (!hasCustomerApprovePrivilege)
            {
                Console.WriteLine("❌ Assessor doesn't have Customer-Approve privilege");
                return false;
            }

            // Test document verification capability
            var hasDocumentVerifyPrivilege = await HasPrivilege(assessor.Id, "Document-Verify");
            if (!hasDocumentVerifyPrivilege)
            {
                Console.WriteLine("❌ Assessor doesn't have Document-Verify privilege");
                return false;
            }

            Console.WriteLine($"✅ Assessor has required privileges for customer approval and document verification");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Assessor flow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestCustomerFlow()
    {
        try
        {
            var customer = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "customer");

            if (customer == null)
            {
                Console.WriteLine("❌ Customer user not found");
                return false;
            }

            // Test service request capability
            var hasServiceRequestPrivilege = await HasPrivilege(customer.Id, "Customer-CreateServiceRequest");
            if (!hasServiceRequestPrivilege)
            {
                Console.WriteLine("❌ Customer doesn't have Customer-CreateServiceRequest privilege");
                return false;
            }

            // Test message sending capability
            var hasMessageSendPrivilege = await HasPrivilege(customer.Id, "Message-Send");
            if (!hasMessageSendPrivilege)
            {
                Console.WriteLine("❌ Customer doesn't have Message-Send privilege");
                return false;
            }

            Console.WriteLine($"✅ Customer has required privileges for service requests and messaging");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Customer flow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestManagerFlow()
    {
        try
        {
            var manager = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "manager");

            if (manager == null)
            {
                Console.WriteLine("❌ Manager user not found");
                return false;
            }

            // Test service assignment capability
            var hasServiceAssignPrivilege = await HasPrivilege(manager.Id, "Service-Assign");
            if (!hasServiceAssignPrivilege)
            {
                Console.WriteLine("❌ Manager doesn't have Service-Assign privilege");
                return false;
            }

            // Test service oversight capability
            var hasServiceGetAllPrivilege = await HasPrivilege(manager.Id, "Service-GetAll");
            if (!hasServiceGetAllPrivilege)
            {
                Console.WriteLine("❌ Manager doesn't have Service-GetAll privilege");
                return false;
            }

            Console.WriteLine($"✅ Manager has required privileges for service assignment and oversight");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Manager flow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestCaseExecutorFlow()
    {
        try
        {
            var caseExecutor = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "caseexecutor");

            if (caseExecutor == null)
            {
                Console.WriteLine("❌ CaseExecutor user not found");
                return false;
            }

            // Test service status update capability
            var hasServiceUpdateStatusPrivilege = await HasPrivilege(caseExecutor.Id, "Service-UpdateStatus");
            if (!hasServiceUpdateStatusPrivilege)
            {
                Console.WriteLine("❌ CaseExecutor doesn't have Service-UpdateStatus privilege");
                return false;
            }

            // Test document upload capability
            var hasDocumentUploadPrivilege = await HasPrivilege(caseExecutor.Id, "Document-Upload");
            if (!hasDocumentUploadPrivilege)
            {
                Console.WriteLine("❌ CaseExecutor doesn't have Document-Upload privilege");
                return false;
            }

            Console.WriteLine($"✅ CaseExecutor has required privileges for service execution and document management");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CaseExecutor flow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestCompleteWorkflow()
    {
        try
        {
            // This test simulates the complete MOT service workflow
            Console.WriteLine("  🔄 Simulating complete MOT service workflow...");

            // Step 1: DataEncoder creates a customer
            var dataEncoder = await _context.Users.FirstOrDefaultAsync(u => u.Username == "dataencoder");
            var customer = await CreateTestCustomer(dataEncoder.Id);
            if (customer == null)
            {
                Console.WriteLine("  ❌ Failed to create test customer");
                return false;
            }
            Console.WriteLine($"  ✅ Created test customer: {customer.BusinessName}");

            // Step 2: Assessor approves the customer
            var assessor = await _context.Users.FirstOrDefaultAsync(u => u.Username == "assessor");
            customer.Verify(assessor.Id, "Test approval");
            await _context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Customer approved by assessor");

            // Step 3: Customer creates a service request
            var service = await CreateTestService(customer.Id, dataEncoder.Id);
            if (service == null)
            {
                Console.WriteLine("  ❌ Failed to create test service");
                return false;
            }
            Console.WriteLine($"  ✅ Created test service: {service.ServiceNumber}");

            // Step 4: Manager assigns service to CaseExecutor
            var manager = await _context.Users.FirstOrDefaultAsync(u => u.Username == "manager");
            var caseExecutor = await _context.Users.FirstOrDefaultAsync(u => u.Username == "caseexecutor");
            service.AssignCaseExecutor(caseExecutor.Id);
            await _context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Service assigned to CaseExecutor");

            // Step 5: CaseExecutor updates service status
            service.UpdateStatus(ServiceStatus.InProgress);
            await _context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Service status updated to InProgress");

            // Step 6: CaseExecutor completes the service
            service.UpdateStatus(ServiceStatus.Completed);
            await _context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Service completed");

            // Clean up test data
            _context.Services.Remove(service);
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Test data cleaned up");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Complete workflow test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestDocumentManagement()
    {
        try
        {
            // Test document-related entities and relationships
            var serviceDocuments = await _context.ServiceDocuments.CountAsync();
            var stageDocuments = await _context.StageDocuments.CountAsync();
            var customerDocuments = await _context.CustomerDocuments.CountAsync();

            Console.WriteLine($"  📄 Document entities accessible: ServiceDocuments={serviceDocuments}, StageDocuments={stageDocuments}, CustomerDocuments={customerDocuments}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Document management test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestMessagingSystem()
    {
        try
        {
            // Test messaging entities and relationships
            var serviceMessages = await _context.ServiceMessages.CountAsync();
            var notifications = await _context.Notifications.CountAsync();

            Console.WriteLine($"  💬 Messaging entities accessible: ServiceMessages={serviceMessages}, Notifications={notifications}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Messaging system test failed: {ex.Message}");
            return false;
        }
    }

    private async Task<Customer?> CreateTestCustomer(long dataEncoderId)
    {
        try
        {
            var customer = Customer.Create(
                "Test Business",
                "TIN123456789",
                "LIC123456789",
                "123 Test Street",
                "Test City",
                "Test State",
                "12345",
                "Test Contact",
                "+1234567890",
                "test@business.com",
                "Retail", // businessType
                "LIC123456789", // importLicense
                DateTime.UtcNow.AddYears(1), // importLicenseExpiry
                dataEncoderId, // userId
                dataEncoderId // createdByDataEncoderId
            );

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Service?> CreateTestService(long customerId, long dataEncoderId)
    {
        try
        {
            var serviceNumber = $"TEST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var service = Service.Create(
                serviceNumber,
                "Test Item Description",
                "Test Route Category",
                1000.00m,
                "Test Tax Category",
                "Test Country",
                ServiceType.Multimodal,
                customerId,
                dataEncoderId
            );

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            // Create service stages
            var stages = new[]
            {
                ServiceStageExecution.Create(service.Id, ServiceStage.PrepaymentInvoice),
                ServiceStageExecution.Create(service.Id, ServiceStage.DropRisk),
                ServiceStageExecution.Create(service.Id, ServiceStage.DeliveryOrder)
            };

            _context.ServiceStages.AddRange(stages);
            await _context.SaveChangesAsync();

            return service;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> HasPrivilege(long userId, string privilegeAction)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePrivileges)
            .ThenInclude(rp => rp.Privilege)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        return user.UserRoles
            .SelectMany(ur => ur.Role.RolePrivileges)
            .Any(rp => rp.Privilege.Action == privilegeAction);
    }

    private string GenerateJwtToken(User user)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super_secure_secret_key_change_this_later_development_only"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("userName", user.Username),
                new Claim("email", user.Email),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var token = new JwtSecurityToken(
                issuer: "TransitPortal",
                audience: "TransitPortal",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch
        {
            return string.Empty;
        }
    }
}

public class TestResults
{
    public bool DataSeeding { get; set; }
    public bool SuperAdminFlow { get; set; }
    public bool DataEncoderFlow { get; set; }
    public bool AssessorFlow { get; set; }
    public bool CustomerFlow { get; set; }
    public bool ManagerFlow { get; set; }
    public bool CaseExecutorFlow { get; set; }
    public bool CompleteWorkflow { get; set; }
    public bool DocumentManagement { get; set; }
    public bool MessagingSystem { get; set; }
    public bool OverallSuccess { get; set; }

    public string GetSummary()
    {
        var passed = new List<string>();
        var failed = new List<string>();

        if (DataSeeding) passed.Add("Data Seeding"); else failed.Add("Data Seeding");
        if (SuperAdminFlow) passed.Add("SuperAdmin Flow"); else failed.Add("SuperAdmin Flow");
        if (DataEncoderFlow) passed.Add("DataEncoder Flow"); else failed.Add("DataEncoder Flow");
        if (AssessorFlow) passed.Add("Assessor Flow"); else failed.Add("Assessor Flow");
        if (CustomerFlow) passed.Add("Customer Flow"); else failed.Add("Customer Flow");
        if (ManagerFlow) passed.Add("Manager Flow"); else failed.Add("Manager Flow");
        if (CaseExecutorFlow) passed.Add("CaseExecutor Flow"); else failed.Add("CaseExecutor Flow");
        if (CompleteWorkflow) passed.Add("Complete Workflow"); else failed.Add("Complete Workflow");
        if (DocumentManagement) passed.Add("Document Management"); else failed.Add("Document Management");
        if (MessagingSystem) passed.Add("Messaging System"); else failed.Add("Messaging System");

        return $"✅ Passed ({passed.Count}): {string.Join(", ", passed)}\n❌ Failed ({failed.Count}): {string.Join(", ", failed)}";
    }
}
