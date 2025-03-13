using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BankingServices.Models;
using ODataConventionModelBuilder = Microsoft.OData.ModelBuilder.ODataConventionModelBuilder;

namespace BankingServices.Configurations
{
    public static class ODataConfig
    {
        
        public static void ConfigureOData(IServiceCollection services)
        {
            
            services.AddControllers()
                .AddOData(options =>
                {
                    // Enable various OData features
                    options.Select()
                        .Filter()
                        .OrderBy()
                        .Expand()
                        .SetMaxTop(200)
                        .Count();
                });
            
        }

        
        private static Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<TransactionLog>("TransactionLogs");
            // maybe other entities later
            return builder.GetEdmModel();
        }
    }
}