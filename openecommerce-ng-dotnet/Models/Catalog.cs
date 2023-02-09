namespace openecommerce_ng_dotnet.Models;

public class ItemData {
    public string code {get; set;} = "";
    public string description {get; set; } = "";
    public double price {get; set; } = 0.0;
}

public class CatalogRequest {
  public string filter {get; set;} = "";
  public int recordsPerPage {get; set;} = 10;
  public int pageNumber {get; set;} = 0;
}