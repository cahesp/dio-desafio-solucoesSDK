using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ProjetoDesafio2
{
    public class PluginAccountPreOperation : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = 
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = 
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);

            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entidadeContexto = (Entity)context.InputParameters["Target"];

                if (entidadeContexto.LogicalName == "account")
                {
                    if (entidadeContexto.Attributes.Contains("telephone1"))
                    {
                        var phone1 = entidadeContexto["telephone1"].ToString();

                        string FetchContact = @"<fetch version = '1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                        "<entity name='contact'>" +
                        "<attribute name='fullname'/>" +
                        "<attribute name='telephone1'/>" +
                        "<attribute name='contactid'/>" +
                        "<order attribute='fullname' descending='false'/>" +
                        "<filter type='and'>" +
                        "<condition attribute='telephone1' operator='eq' value='" + phone1 + "'/>" +
                        "</filter>" +
                        "</entity>" +
                        "</fetch>";

                        trace.Trace("FetchContact: " + FetchContact);

                        var primarycontact = serviceAdmin.RetrieveMultiple(new FetchExpression(FetchContact));

                        if (primarycontact.Entities.Count > 0)
                        {
                            foreach ( var entityContact in primarycontact.Entities)
                            {
                                entidadeContexto["primarycontactid"] = new EntityReference("contact", entityContact.Id);
                            }
                        }

                    }
                }
            }
        }
    }
}
