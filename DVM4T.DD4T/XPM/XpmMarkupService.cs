using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;
using Dynamic = DD4T.ContentModel;
using DD4T.Mvc.SiteEdit;
using DD4T.ContentModel;

namespace DVM4T.DD4T.XPM
{
    /// <summary>
    /// XPM Markup Service for the DD4T DVM4T implementation
    /// </summary>
    /// <remarks>Does not support use of other DVM4T Implementations</remarks>
    public class XpmMarkupService : IXpmMarkupService
    {
        private Dynamic.IField CreateDD4TField(IFieldData field)
        {
            //Dynamic.Schema schema = field.EmbeddedSchema != null ?
            //    new Dynamic.Schema
            //    {
            //        RootElementName = field.EmbeddedSchema.RootElementName,
            //        Title = field.EmbeddedSchema.Title,
            //        Id = field.EmbeddedSchema.TcmUri
            //    }
            //    : new Dynamic.Schema();
            //return new Dynamic.Field
            //{
            //    XPath = field.XPath,
            //    Name = field.Name,
            //    EmbeddedSchema = schema
            //};
            return field == null ? null : field.BaseData as Dynamic.IField; //Will only work with DD4T implementation
        }
        public string RenderXpmMarkupForField(IFieldData field, int index = -1)
        {
            var dd4tField = CreateDD4TField(field);
            var result = index >= 0 ? SiteEditService.GenerateSiteEditFieldTag(dd4tField, index)
                            : SiteEditService.GenerateSiteEditFieldTag(dd4tField);
            return result ?? string.Empty;
        }

        public string RenderXpmMarkupForComponent(IContentPresentationData cp, string region = null)
        {
            var dd4tCP = cp == null ? null : cp.BaseData as IComponentPresentation; //Will only work with DD4T implementation
            //new Dynamic.ComponentPresentation
            //{
            //    Component = new Dynamic.Component
            //    {
            //        Id = cp.Component.TcmUri,
            //        Title = cp.Component.Title,
            //        PublicationId = "tcm:0-" + cp.Component.PublicationId + "-1"
            //    },
            //    ComponentTemplate = new Dynamic.ComponentTemplate
            //    {
            //        Title = cp.Template.Title,
            //        Id = cp.Template.TcmUri,
            //        RevisionDate = cp.Template.RevisionDate
            //    }
            //};
            return SiteEditService.GenerateSiteEditComponentTag(dd4tCP, region); 
        }

        public bool IsSiteEditEnabled(ITridionItemData item)
        {
            int publicationId = 0;
            try
            {
                publicationId = new TcmUri(item.TcmUri).PublicationId;
            }
            catch (Exception) 
            {
                publicationId = 0;
            }
            return IsSiteEditEnabled(publicationId);
        }

        public bool IsSiteEditEnabled(int publicationId)
        {
            var settings = SiteEditService.SiteEditSettings.FirstOrDefault(x => x.Key == publicationId.ToString());
            return settings.Value == null ? false : settings.Value.Enabled;
        }
    }
}
