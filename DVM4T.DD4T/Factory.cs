using Dynamic = DD4T.ContentModel;
using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVM4T.DD4T
{
    public class DefaultModelDataFactory : IModelDataFactory
    {
        public IComponentData GetModelData(Dynamic.IComponent component)
        {
            return new Component(component);
        }

        public IComponentPresentationData GetModelData(Dynamic.IComponentPresentation cp)
        {
            return new ComponentPresentation(cp);
        }

        public IContentPresentationData GetModelData(Dynamic.IFieldSet fields, Dynamic.ISchema schema, Dynamic.IComponentTemplate template)
        {
            return new ContentPresentationData(new FieldSet(fields), new Schema(schema), new ComponentTemplate(template));
        }

        public IPageData GetModelData(Dynamic.IPage page)
        {
            return new Page(page);
        }

        public IKeywordData GetModelData(Dynamic.IKeyword keyword, string categoryName)
        {
            return new Keyword(keyword, categoryName);
        }
    }

    //public class DIModelFactory : IModelDataFactory
    //{
    //    private IModelDataResolver resolver;
    //    public DIModelFactory(IModelDataResolver resolver)
    //    {
    //        this.resolver = resolver;
    //    }

    //    public IComponentPresentationData GetModelData(Dynamic.IComponentPresentation cp)
    //    {
    //        return resolver.GetViewModelData<IComponentPresentationData>(cp);
    //    }

    //    public IContentPresentationData GetModelData(Dynamic.IFieldSet fields, Dynamic.ISchema schema, Dynamic.IComponentTemplate template)
    //    {
    //        return resolver.GetViewModelData<IContentPresentationData>(fields, schema, template);
    //    }

    //    public IPageData GetModelData(Dynamic.IPage page)
    //    {
    //        return resolver.GetViewModelData<IPageData>(page);
    //    }

    //    public IKeywordData GetModelData(Dynamic.IKeyword keyword)
    //    {
    //        return resolver.GetViewModelData<IKeywordData>(keyword);
    //    }

    //    public IComponentData GetModelData(Dynamic.IComponent component)
    //    {
    //        return resolver.GetViewModelData<IComponentData>(component);
    //    }
    //}

    public class DefaultDD4TFactory : IDD4TFactory
    {
        public Dynamic.IComponentPresentation BuildComponentPresentation(IComponentData component, ITemplateData componentTemplate)
        {
            return new Dynamic.ComponentPresentation //Locking in to default DD4T implementation
            {
                Component = component.BaseData as Dynamic.Component, //Assuming default DD4T implementation
                ComponentTemplate = componentTemplate.BaseData as Dynamic.ComponentTemplate //Assuming default DD4T implementation
            };
        }
    }

    public interface IModelDataResolver
    {
        T GetViewModelData<T>(params object[] data) where T : IViewModelData;
    }
    /// <summary>
    /// Factory for creating View Model Data using DD4T objects
    /// </summary>
    public interface IModelDataFactory
    {
        IComponentData GetModelData(Dynamic.IComponent component);
        IComponentPresentationData GetModelData(Dynamic.IComponentPresentation cp);
        IContentPresentationData GetModelData(Dynamic.IFieldSet fields, Dynamic.ISchema schema, Dynamic.IComponentTemplate template);
        IPageData GetModelData(Dynamic.IPage page);
        IKeywordData GetModelData(Dynamic.IKeyword keyword, string categoryName);
    }
    /// <summary>
    /// Factory for creating DD4T Objects
    /// </summary>
    public interface IDD4TFactory
    {
        Dynamic.IComponentPresentation BuildComponentPresentation(IComponentData component, ITemplateData componentTemplate);
    }

    /// <summary>
    /// Static Container for all dependencies of this Library. All Properties are initialized with default implementations.
    /// </summary>
    public static class Dependencies
    {
        private static IModelDataFactory factory = new DefaultModelDataFactory();
        private static IDD4TFactory dd4tFactory = new DefaultDD4TFactory();
        //For greater flexibility could also use something like: Func<IDD4TFactory> dd4tFactory = () => new DefaultDD4TFactory();
        /// <summary>
        /// The Model Data Factory used internally by this Library
        /// </summary>
        /// <remarks>Primarily used to get IViewModelData instances in the Attribute GetPropertyValue overrides.</remarks>
        public static IModelDataFactory DataFactory
        {
            get
            {
                return factory;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("DataFactory");
                factory = value;
            }
        }
        /// <summary>
        /// The DD4T Factory used internally by this Library
        /// </summary>
        public static IDD4TFactory DD4TFactory
        {
            get
            {
                return dd4tFactory;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("DD4TFactory");
                dd4tFactory = value;
            }
        }
    }
}
