using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transit.Application;

public class DashboardResponse
{
    public long? NumberOfStores { get; set; }
    public long? NumberOfAgents { get; set; }
    public long? NumberOfProperties { get; set; }
    public long? NumberOfOrders { get; set; }

    // New properties
    public long? ActiveListings { get; set; }
    public double? ListingsTurnoverRate { get; set; }
    public long? TotalTransactions { get; set; }
    public double? TotalRevenue { get; set; }
    public decimal? StoreRevenue { get; set; }
    public decimal? TotalRevenueInSubCity { get; set; }
    public decimal? MarketShare { get; set; }
    public long? TotalInquiries { get; set; }
    public long? ResolvedInquiries { get; set; }
    public long? InactiveAgents { get; set; }
    public double? TotalRevenues { get; set; }
    public long? TotalCompletedTransactions { get; set; }
    public double? AvgRevenuePerTransaction { get; set; }
    public double? InquiryClosureRate { get; set; }
    public long? TotalAgents { get; set; }
    public long? ActiveAgents { get; set; }
    public double? AgentUtilization { get; set; }
    public long? StoreProperties { get; set; }
    public long? TotalPropertiesInCity { get; set; }
    public double? ServiceCoverage { get; set; }


    public List<AgentCommissionDistribution> AgentCommissionDistribution { get; set; }
    public List<PropertyTypesBreakdown> PropertyTypesBreakdown { get; set; }
    public List<PopularListing> PopularListings { get; set; }
    public List<MonthlyRevenue> MonthlyRevenue { get; set; }
    public List<CROUtilization> CROUtilization { get; set; }
    public List<AgentPerformance> AgentPerformance { get; set; }
    public List<OperatorPerformance> OperatorPerformance { get; set; }
}

public class AgentCommissionDistribution
{
    public long? AgentId { get; set; }
    public double? TotalCommission { get; set; }  // Use double? here instead of float
}


public class PropertyTypesBreakdown
{
    public string PropertyType { get; set; }
    public long? Count { get; set; }
}

public class PopularListing
{
    public long? Id { get; set; }
    public string PropertyType { get; set; }
    public int NumberOfView { get; set; }
}

public class MonthlyRevenue
{
    public int Year { get; set; }
    public int Month { get; set; }
    public double? TotalRevenue { get; set; }
}

public class CROUtilization
{
    public long? CROId { get; set; }
    public int InquiriesHandled { get; set; }
}

public class AgentPerformance
{
    public long? UserId { get; set; }
    public string Name { get; set; }
    public long? ListingsCreated { get; set; }
    public long? PropertiesSold { get; set; }
    public long? InquiriesHandled { get; set; }
}

public class OperatorPerformance
{
    public long? UserId { get; set; }
    public string Name { get; set; }
    public long? InquiriesCreated { get; set; }
    public long? CompletedInquiries { get; set; }
    public long? ActiveInquiries { get; set; }
    public long? AverageInquiriesPerDay { get; set; }
    public double? OrderShareFromOperators { get; set; }
}
