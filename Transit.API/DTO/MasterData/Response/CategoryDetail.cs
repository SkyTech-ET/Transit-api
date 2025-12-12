namespace Transit.API;

public class CategoryDetail
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<long> PropertyIds { get; set; }

    public CategoryDetail()
    {
        PropertyIds = new List<long>();
    }
}
