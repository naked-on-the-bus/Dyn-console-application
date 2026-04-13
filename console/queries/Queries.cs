using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using EC = Microsoft.Xrm.Sdk.EntityCollection;

namespace Query;

public class Queries
{
    private IOrganizationService service;
    public Queries(IOrganizationService service) => this.service = service;

    // your queries here 
    // example 
    /*
    public EC getAccounts()
    {

        var query = new QueryExpression("account");
        query.ColumnSet.AddColumns("name", "accountnumber");
        var result = this.service.RetrieveMultiple(query);
        return result;
        

    }
    */
}
