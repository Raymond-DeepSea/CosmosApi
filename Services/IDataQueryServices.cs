using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace CosmosApi.Services
{
    public interface IDataQueryServices
    {
        List<dynamic> QueryCosmosData();

        dynamic QueryCosmosData(string drawDate);
    }
}