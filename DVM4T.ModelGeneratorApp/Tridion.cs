using DVM4T.Base;
using DVM4T.ModelGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace DVM4T.ModelGeneratorApp
{
    public class CoreServiceModelBuilder
    {
        private ReadOptions expandedOptions = new ReadOptions() { LoadFlags = LoadFlags.Expanded };
        private string coreServiceAddress, username, password;
        
        public CoreServiceModelBuilder(string coreServiceAddress, string username, string password)
        {
            this.coreServiceAddress = coreServiceAddress;
            this.username = username;
            this.password = password;
        }
        private SessionAwareCoreServiceClient GetClient()
        {
            //string coreServiceAddress = coreServiceAddress + "/webservices/CoreService2011.svc/wsHttp"; //only will work with 2011 Core Service
            WSHttpBinding binding = new WSHttpBinding();
            binding.TransactionFlow = true;
            binding.ReaderQuotas.MaxStringContentLength = 65536; //8x default
            EndpointAddress address = new EndpointAddress(coreServiceAddress);

            //using (SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient(binding, address))
            //"using" statement results in CommunicationObjectFaultedException when any Exception occurs within it

            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient(binding, address);
            client.ClientCredentials.Windows.ClientCredential.UserName = username;
            client.ClientCredentials.Windows.ClientCredential.Password = password;
            return client;
        }

        public IList<ViewModel> CoreCreateModels(string tcmId)
        {
            IList<ViewModel> result = new List<ViewModel>();
            var client = GetClient();

            OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData()
            {
                ItemTypes = new[] { ItemType.Schema },
                Recursive = true,
                SchemaPurposes = new[] { SchemaPurpose.Component, SchemaPurpose.Embedded } //Filter doesn't work. Schema Purpose is always null
            };
            try
            {
                if (!client.IsExistingObject(tcmId))
                {
                    throw new ArgumentException(
                        String.Format("Could not find Tridion Organizational Item with ID {0}.", tcmId), "tcmId");
                }
                else
                {
                    //Nearly identical to CS 2012 but Tridion.ContentManager.CoreService.Client is not the same between 2011 and 2012 so code is not interchangeable without specifying namespace on everything
                    XNode[] schemas = client.GetListXml(tcmId, filter).Nodes().ToArray();
                    
                    foreach (XElement xSchema in client.GetListXml(tcmId, filter).Nodes())
                    {
                        SchemaData schema = client.Read(xSchema.Attribute("ID").Value, expandedOptions) as SchemaData;
                        var model = CoreCreateModel(schema, client);
                        if (model != null) result.Add(model);
                    }
                }
            }
            finally
            {
                //Cannot dispose of Client with "using" statement if a FaultException occurs. Implementation of IDisposable is flawed in ClientBase
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                if (client.State != CommunicationState.Closed)
                {
                    client.Close();
                }
                client = null;
            }
            return result;
        }


        private ViewModel CoreCreateModel(SchemaData schema, SessionAwareCoreServiceClient client)
        {
            Console.WriteLine("\nTitle:" + schema.Title + ", Schema Purpose: " + schema.Purpose);
            IList<FieldProperty> modelProperties = new List<FieldProperty>();
            ViewModel model = new ViewModel();
            //TODO - Why is schema.Purpose always null?! Documentation indicates it should contain the type of Schema
            switch (schema.Purpose)
            {
                case SchemaPurpose.Embedded:
                    model.BaseClass = typeof(EmbeddedSchemaViewModelBase);
                    break;
                case SchemaPurpose.Component:
                    model.BaseClass = typeof(ComponentPresentationViewModelBase);
                    break;
                case SchemaPurpose.Metadata:
                    return null;
                case SchemaPurpose.Multimedia:
                    model.BaseClass = typeof(ComponentPresentationViewModelBase); //testing
                    break;
                case SchemaPurpose.Protocol:
                    return null;
                case SchemaPurpose.TemplateParameters:
                    return null;
                case SchemaPurpose.UnknownByClient:
                    return null;
                case SchemaPurpose.VirtualFolderType:
                    return null;
                case SchemaPurpose.Bundle:
                    return null;
                default:
                    break;
            }

            SchemaFieldsData fields = client.ReadSchemaFields(schema.Id, false, expandedOptions);

            if (fields.Fields != null)
            {
                ProcessFields(fields.Fields, false, ref modelProperties);   
            }
            if (fields.MetadataFields != null)
            {
                ProcessFields(fields.MetadataFields, true, ref modelProperties);
            }
            model.Name = schema.Title.ResolveModelName();
            model.SchemaName = schema.Title;
            model.FieldProperties = modelProperties;
            model.ContainingFolder = schema.LocationInfo.OrganizationalItem.Title.ResolveModelName();
            return model;
        }
        private static void ProcessFields(ItemFieldDefinitionData[] fields, bool isMetadata, ref IList<FieldProperty> modelProperties)
        {
            foreach (ItemFieldDefinitionData field in fields)
            {
                var fieldProp = new FieldProperty();
                fieldProp.IsMultiValue = field.MaxOccurs != 1;
                fieldProp.IsMetadata = isMetadata;
                fieldProp.FieldName = field.Name;
                fieldProp.Name = field.Name.ResolvePropertyName();
                if (field is ComponentLinkFieldDefinitionData)
                {
                    //Linked component
                    fieldProp.FieldType = FieldType.Linked;
                    ComponentLinkFieldDefinitionData myField = field as ComponentLinkFieldDefinitionData;
                    IList<string> linkedClassNames = new List<string>();
                    IList<FieldProperty> linkedModelProperties = new List<FieldProperty>();
                    if (myField.AllowedTargetSchemas != null)
                    {
                        foreach (LinkToSchemaData link in myField.AllowedTargetSchemas)
                        {
                            fieldProp.LinkedComponentTypeNames.Add(link.Title.ResolveModelName());
                        }
                    }
                }
                else if (field is EmbeddedSchemaFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Embedded;
                    EmbeddedSchemaFieldDefinitionData myField = field as EmbeddedSchemaFieldDefinitionData;
                    if (myField.EmbeddedSchema != null)
                    {
                        fieldProp.EmbeddedTypeName = myField.EmbeddedSchema.Title.ResolveModelName();
                    }
                }
                else if (field is MultiLineTextFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Text;
                }
                else if (field is KeywordFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Keyword;
                }
                else if (field is NumberFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Number;
                }
                else if (field is DateFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Date;
                }
                else if (field is ExternalLinkFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.ExternalLink;
                }
                else if (field is MultimediaLinkFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Multimedia;
                }
                else if (field is SingleLineTextFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.Text;
                }
                else if (field is XhtmlFieldDefinitionData)
                {
                    fieldProp.FieldType = FieldType.RichText;
                }
                else if (field is ItemFieldDefinitionData)
                {
                    //"Default"
                    Console.WriteLine("Unknown type for Field: " + field.Name);
                    continue; //don't add the property
                }
                modelProperties.Add(fieldProp);
            }
        }
    }
}
