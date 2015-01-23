using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;

namespace DVM4T.Base
{
    /// <summary>
    /// Base class for all Component Presentation View Models
    /// </summary>
    public abstract class ComponentPresentationViewModelBase : IComponentPresentationViewModel
    {
        private IFieldSet fields = null;
        private IFieldSet metadataFields = null;
        public IComponentPresentation ComponentPresentation
        {
            get;
            set;
        }
        public IViewModelBuilder Builder
        {
            get;
            set;
        }
        public IFieldSet Fields
        {
            get
            {
                if (fields == null && ComponentPresentation != null && ComponentPresentation.Component != null)
                {
                    fields = ComponentPresentation.Component.Fields;
                }
                return fields;
            }
            set
            {
                fields = value;
            }
        }
        public IFieldSet MetadataFields
        {
            get
            {
                if (metadataFields == null && ComponentPresentation != null && ComponentPresentation.Component != null)
                {
                    metadataFields = ComponentPresentation.Component.MetadataFields;
                }
                return metadataFields;
            }
        }
        public int PublicationId
        {
            get
            {
                try
                {
                    return ComponentPresentation.Component.PublicationId;
                }
                catch
                {
                    return 0;
                }
            }
        }


        public IComponentTemplate ComponentTemplate
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Base class for all Embedded Schema View Models
    /// </summary>
    public abstract class EmbeddedSchemaViewModelBase : IEmbeddedSchemaViewModel
    {
        public IComponentTemplate ComponentTemplate
        {
            get;
            set;
        }
        public IViewModelBuilder Builder
        {
            get;
            set;
        }
        public IFieldSet Fields
        {
            get;
            set;
        }
        public IFieldSet MetadataFields
        {
            get
            {
                return null;
            }
        }
        public int PublicationId
        {
            get
            {
                try
                {
                    return ComponentTemplate.PublicationId;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}
