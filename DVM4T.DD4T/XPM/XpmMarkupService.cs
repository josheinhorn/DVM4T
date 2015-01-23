using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;
using Dynamic = DD4T.ContentModel;
using DD4T.Mvc.SiteEdit;

namespace DVM4T.DD4T.XPM
{

    public class XpmMarkupService : IXpmMarkupService
    {
        private Dynamic.Field CreateDD4TField(IField field)
        {
            Dynamic.Schema schema = field.EmbeddedSchema != null ?
                new Dynamic.Schema
                {
                    RootElementName = field.EmbeddedSchema.RootElementName,
                    Title = field.EmbeddedSchema.Title,
                    Id = field.EmbeddedSchema.TcmUri
                }
                : new Dynamic.Schema();
            return new Dynamic.Field
            {
                XPath = field.XPath,
                Name = field.Name,
                EmbeddedSchema = schema
            };
        }
        public string RenderXpmMarkupForField(IField field, int index = -1)
        {
            var dd4tField = CreateDD4TField(field);
            var result = index >= 0 ? SiteEditService.GenerateSiteEditFieldTag(dd4tField, index)
                            : SiteEditService.GenerateSiteEditFieldTag(dd4tField);
            return result ?? string.Empty;
        }

        public string RenderXpmMarkupForComponent(IComponentPresentation cp, string region = null)
        {
            var dd4tCP = new Dynamic.ComponentPresentation
            {
                Component = new Dynamic.Component
                {
                    Id = cp.Component.TcmUri,
                    Title = cp.Component.Title,
                    PublicationId = "tcm:0-" + cp.Component.PublicationId + "-1"
                },
                ComponentTemplate = new Dynamic.ComponentTemplate
                {
                    Title = cp.ComponentTemplate.Title,
                    Id = cp.ComponentTemplate.TcmUri,
                    RevisionDate = cp.ComponentTemplate.RevisionDate
                }
            };
            return SiteEditService.GenerateSiteEditComponentTag(dd4tCP, region);
        }

        public bool IsSiteEditEnabled(ITridionItem item)
        {
            return IsSiteEditEnabled(item.PublicationId);
        }

        public bool IsSiteEditEnabled(int publicationId)
        {
            var settings = SiteEditService.SiteEditSettings.FirstOrDefault(x => x.Key == publicationId.ToString());
            return settings.Value == null ? false : settings.Value.Enabled;
        }
    }
}
