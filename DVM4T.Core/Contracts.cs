using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace DVM4T.Contracts
{
    #region XPathable Data - probably won't use
    public interface IXPathableNode
    {
        IXPathableNode SelectSingleNode(string xpath);
        IXPathableNode[] SelectNodes(string xpath);
        string Name { get; }
        string Value { get; }
        IAttribute[] Attributes { get; }
        IXPathableNode ParentNode { get; }
        IXPathableNode[] GetAllChildNodes(); //necessary?
        string GetAsXmlString();
    }
    //Need to be able to retrieve Attributes via XPath as well
    public interface IAttribute
    {
        string Key { get; set; }
        string Value { get; set; }
    }

    public interface IHaveNode
    {
        IXPathableNode Node { get; set; }
    }
    //Is this interface necessary for this implementation? Seems to be connected to specifically an XML implementation
    public interface ITridionXPathProvider
    {
        string GetFieldXPath(string fieldName, int index);

        //Are any of these necessary?
        string GetComponentXPath();
        string GetComponentTemplateXPath();
        string GetFieldSetXPath(string name); //necessary?
        string GetSchemaXPath();
    }

    #endregion

    #region Tridion interfaces


    public interface IHaveData //consider making this generic i.e. IHaveData<T> { T BaseData { get; } } -- would require a lot of refactoring
    {
        object BaseData { get; }
    }
    public interface ITridionItemData : IHaveData
    {
        string TcmUri { get; }
        string Title { get; }
        int PublicationId { get; }
    }
    public interface IPageData : ITridionItemData
    {
        IList<IComponentPresentationData> ComponentPresentations { get; }
        IPageTemplateData PageTemplate { get; }
        IFieldsData Metadata { get; }
        string FileName { get; }
    }
    public interface IComponentPresentationData : IHaveData
    {
        IComponentData Component { get; }
        IComponentTemplateData ComponentTemplate { get; }
    }

    public interface IComponentData : ITridionItemData
    {
        IFieldsData Fields { get; }
        IFieldsData MetadataFields { get; }
        ISchemaData Schema { get; }
        IMultimediaData MultimediaData { get; }
    }
    public interface ITemplateData : ITridionItemData
    {
        IFieldsData Metadata { get; }
        DateTime RevisionDate { get; }
    }
    public interface IComponentTemplateData : ITemplateData
    {
        //Anything specific to this?
    }

    public interface IPageTemplateData : ITemplateData
    {
        //Anything specific to this?
    }
    public interface IMultimediaData : IHaveData
    {
        string Url { get; }
        string FileName { get; }
        string MimeType { get; }
    }

    public interface IFieldsData : IHaveData
    {
        IFieldData this[string key] { get; } //Convert this to xpath
        bool ContainsKey(string key); //check if the XPath exists
    }

    public interface ISchemaData : ITridionItemData
    {
        string RootElementName { get; }
    }
    public interface ICanUseXpm
    {
        string XPath { get; }
        string Name { get; }
        string RootElementName { get; }
    }
    public interface IFieldData : ICanUseXpm, IHaveData
    {
        //The object Value is an important part of this framework -- this object will be retrieved based on the FieldAttribute's overriden method. Implementer could alternatively use the BaseData object to circumvent
        IEnumerable Values { get; } //should this be object or is it ok as Enumerable?
        ISchemaData EmbeddedSchema { get; }
        string FieldTypeString { get; } //maybe make an enum?? This would prescribe the implementation though -- could use object to make it really generic
    }
    #endregion

    /// <summary>
    /// A View Model
    /// </summary>
    public interface IViewModel
    {
        IViewModelData ModelData { get; set; }
    }

    public interface IContentViewModel : IViewModel
    {
        ISchemaData Schema { get; set; } //This is specific to Component/Embedded
        IFieldsData ContentData { get; set; } //This is specific to Component/Embedded
        //IComponentTemplateData ComponentTemplate { get; set; } //Is this necessary?
    }

    public interface IViewModelData : IHaveData //Should this extend IHaveData? I don't think so at this time because each piece i.e. Content, MetadataFields has their own BaseData object
    {
        /// <summary>
        /// The underlying data object that the View Model represents
        /// </summary>
        IViewModelBuilder Builder { get; }
        //IFieldsData Content { get; } //This is specific to Component/Embedded
        IFieldsData Metadata { get; }
        ITemplateData Template { get; }
        //we required Component Template here in order to generate Site Edit markup for any linked components in the embedded fields
        //IComponentTemplateData ComponentTemplate { get; } //This is specific to Component/Embedded
        /// <summary>
        /// Publication ID of the underlying Tridion item
        /// </summary>
        int PublicationId { get; }
    }

    public interface IContentViewModelData : IViewModelData
    {
        ISchemaData Schema { get; }
        IFieldsData ContentData { get; }
    }

    public interface IComponentPresentationViewModelData : IContentViewModelData
    {
        IComponentPresentationData ComponentPresentation { get; }
    }

    public interface IEmbeddedSchemaViewModelData : IContentViewModelData
    {
        //??
    }

    public interface IPageViewModelData : IViewModelData
    {
        IPageData Page { get; }
    }
    //Consider need for Component Presentation specific Data -- should be taken care of by accessing BaseData though...


    //Removed these 3 ViewModel interfaces and instead using different ViewModelData objects -- investigate the impacts of this
    //public interface IComponentPresentationViewModel : IViewModel
    //{
    //    IComponentPresentationData ComponentPresentation { get; set; }
    //}

    ////Consider use cases for this interface -- is it worth separating from ComponentPresentationViewModel?
    ////Multimedia... could use CP instead, not technically correct but it's close
    //public interface IComponentViewModel : IViewModel
    //{
    //    IComponentData Component { get; }  
    //}

    ////TODO: Consider removing this interface, holding on to template is not actually necessary after building is done
    //public interface IEmbeddedSchemaViewModel : IViewModel
    //{

    //}

    //public interface IPageViewModel : IViewModel
    //{
    //    IPageData Page { get; }
    //}

    public interface IViewModelBuilder
    {
        Type FindViewModelByAttribute<T>(IViewModelData data, Type[] typesToSearch = null) where T : IModelAttribute;
        /// <summary>
        /// Builds a View Model from previously loaded Assemblies using the input Component Presentation. This method infers the View Model type by comparing
        /// information from the Component Presentation object to the attributes of the View Model classes.
        /// </summary>
        /// <param name="cp">Component Presentation object</param>
        /// <remarks>
        /// The LoadViewModels method must be called with the desired View Model Types in order for this to return a valid object.
        /// </remarks>
        /// <returns>Component Presentation View Model</returns>
        IViewModel BuildCPViewModel(IComponentPresentationData componentPresentation); //A way to build view model without passing type -- type is inferred using loaded assemblies
        /// <summary>
        /// Builds a View Model from a FieldSet using the schema determine the View Model class to use.
        /// </summary>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="embeddedSchema">Embedded Schema</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        IViewModel BuildEmbeddedViewModel(IFieldsData embeddedFields, ISchemaData embeddedSchemaTitle, ITemplateData template);
        /// <summary>
        /// Builds a View Model from a Component Presentation using the type parameter to determine the View Model class to use.
        /// </summary>
        /// <param name="type">Type of View Model class to return</param>
        /// <param name="cp">Component Presentation</param>
        /// <returns>Component Presentation View Model</returns>
        IViewModel BuildCPViewModel(Type type, IComponentPresentationData componentPresentation);
        /// <summary>
        /// Builds a View Model from a FieldSet using the generic type to determine the View Model class to use.
        /// </summary>
        /// <param name="type">Type of View Model class to return</param>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        IViewModel BuildEmbeddedViewModel(Type type, IFieldsData embeddedFields, ISchemaData schema, ITemplateData template);
        /// <summary>
        /// Builds a View Model from a Component Presentation using the generic type to determine the View Model class to use.
        /// </summary>
        /// <typeparam name="T">Type of View Model class to return</typeparam>
        /// <param name="cp">Component Presentation</param>
        /// <returns>Component Presentation View Model</returns>
        T BuildCPViewModel<T>(IComponentPresentationData componentPresentation) where T : class, IViewModel;
        /// <summary>
        /// Builds a View Model from a FieldSet using the generic type to determine the View Model class to use.
        /// </summary>
        /// <typeparam name="T">Type of View Model class to return</typeparam>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        T BuildEmbeddedViewModel<T>(IFieldsData embeddedFields, ISchemaData schema, ITemplateData template) where T : class, IViewModel;
        /// <summary>
        /// The View Model Key Provider for this Builder
        /// </summary>
        IViewModelKeyProvider ViewModelKeyProvider { get; }

        T BuildPageViewModel<T>(IPageData page) where T : IViewModel;
        IViewModel BuildPageViewModel(Type type, IPageData page);
        IViewModel BuildPageViewModel(IPageData page);

        /// <summary>
        /// Loads all View Model classes from an assembly
        /// </summary>
        /// <param name="assembly">The Assembly with the view model Types to load</param>
        /// <remarks>
        /// Required for use of builder methods that don't require a Type parameter or generic.
        /// The Builder will only use Types tagged with the ViewModelAttribute class.
        /// </remarks>
        void LoadViewModels(Assembly assembly);
    }

    public interface IXpmMarkupService
    {
        /// <summary>
        /// Renders the XPM Markup for a field
        /// </summary>
        /// <param name="field">Tridion Field</param>
        /// <param name="index">Optional index for multi value fields</param>
        /// <returns>XPM Markup</returns>
        string RenderXpmMarkupForField(IFieldData field, int index = -1);
        /// <summary>
        /// Renders the XPM Markup for a Component Presentation
        /// </summary>
        /// <param name="cp">Component Presentation</param>
        /// <param name="region">Optional region</param>
        /// <returns>XPM Markup</returns>
        string RenderXpmMarkupForComponent(IComponentPresentationData cp, string region = null);
        /// <summary>
        /// Determines if Site Edit is enabled for a particular item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns></returns>
        bool IsSiteEditEnabled(ITridionItemData item);
        /// <summary>
        /// Determines if Site Edit is enabeld for a publication
        /// </summary>
        /// <param name="publicationId">Publication ID</param>
        /// <returns></returns>
        bool IsSiteEditEnabled(int publicationId);
    }

    //TODO: Instantiate and add these to the IViewModel using System.Reflection's MakeGenericType
    public interface IXpmRenderer<TModel> where TModel : IViewModel
    {
        /// <summary>
        /// Renders both XPM Markup and Field Value 
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="index">Optional index for a multi-value field</param>
        /// <returns>XPM Markup and field value</returns>
        HtmlString XpmEditableField<TProp>(Expression<Func<TModel, TProp>> propertyLambda, int index = -1);
        /// <summary>
        /// Renders both XPM Markup and Field Value for a multi-value field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <typeparam name="TItem">Item type - this must match the generic type of the property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="item">The particular value of the multi-value field</param>
        /// <example>
        /// foreach (var content in model.Content)
        /// {
        ///     @model.XpmEditableField(m => m.Content, content);
        /// }
        /// </example>
        /// <returns>XPM Markup and field value</returns>
        HtmlString XpmEditableField<TProp, TItem>(Expression<Func<TModel, TProp>> propertyLambda, TItem item);
        /// <summary>
        /// Renders the XPM markup for a field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="index">Optional index for a multi-value field</param>
        /// <returns>XPM Markup</returns>
        HtmlString XpmMarkupFor<TProp>(Expression<Func<TModel, TProp>> propertyLambda, int index = -1);
        /// <summary>
        /// Renders XPM Markup for a multi-value field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <typeparam name="TItem">Item type - this must match the generic type of the property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="item">The particular value of the multi-value field</param>
        /// <example>
        /// foreach (var content in model.Content)
        /// {
        ///     @model.XpmMarkupFor(m => m.Content, content);
        ///     @content;
        /// }
        /// </example>
        /// <returns>XPM Markup</returns>
        HtmlString XpmMarkupFor<TProp, TItem>(Expression<Func<TModel, TProp>> propertyLambda, TItem item);
        /// <summary>
        /// Renders the XPM Markup for a Component Presentation
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="region">Region</param>
        /// <returns>XPM Markup</returns>
        HtmlString StartXpmEditingZone(string region = null);
    }

    /// <summary>
    /// Provides the View Model Key using a Component Template
    /// </summary>
    public interface IViewModelKeyProvider
    {
        /// <summary>
        /// Retrieves a View Model Key based on a Component Template. Should return the same key for the same template every time.
        /// Return values of null or empty string will be ignored.
        /// </summary>
        /// <param name="template">Component Template</param>
        /// <returns>View Model Key</returns>
        string GetViewModelKey(IViewModelData model);
    }

    public interface ICanBeBoolean
    {
        bool IsBooleanValue { get; set; }
    }

    public interface IReflectionHelper
    {
        List<ModelAttributeProperty> GetModelProperties(Type type);
        T GetCustomAttribute<T>(Type type) where T : IModelAttribute;
        object CreateInstance(Type objectType);
        T CreateInstance<T>() where T : class, new();
        Action<object, object> BuildSetter(PropertyInfo propertyInfo);
        Func<object, object> BuildGetter(PropertyInfo propertyInfo);
        PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda);
        PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda);
    }
    /// <summary>
    /// Data structure for efficient use of Properties marked with a Field Custom Attribute
    /// </summary>
    public struct ModelAttributeProperty
    {
        public string Name { get; set; }
        public Action<object, object> Set { get; set; }
        public Func<object, object> Get { get; set; }
        public IPropertyAttribute PropertyAttribute { get; set; }
        public Type PropertyType { get; set; }
    }
    public interface IPropertyAttribute
    {
        Type ExpectedReturnType { get; }
        object GetPropertyValue(IViewModel model, Type propertyType, IViewModelBuilder builder = null);
    }
    public interface IFieldAttribute : IPropertyAttribute
    {
        object GetFieldValue(IFieldData field, Type propertyType, ITemplateData template, IViewModelBuilder builder = null);
        string FieldName { get; }
        bool AllowMultipleValues { get; set; }
        bool InlineEditable { get; set; }
        bool Mandatory { get; set; }
        bool IsMetadata { get; set; }
        bool IsTemplateMetadata { get; set; }
    }

    //TODO: Anyway to merge all three interfaces into one? They're so similar
    //TODO: Use these interfaces in the builder
    public interface IComponentAttribute : IPropertyAttribute
    {
        object GetPropertyValue(IComponentData component, Type propertyType, IComponentTemplateData template, IViewModelBuilder builder = null);
    }
    public interface IComponentTemplateAttribute : IPropertyAttribute
    {
        object GetPropertyValue(IComponentTemplateData template, Type propertyType, IViewModelBuilder builder = null);
    }
    public interface IPageAttribute : IPropertyAttribute
    {
        object GetPropertyValue(IPageData page, Type propertyType, IViewModelBuilder builder = null);
    }
    public interface IPageTemplateAttribute : IPropertyAttribute
    {
        object GetPropertyValue(IPageTemplateData page, Type propertyType, IViewModelBuilder builder = null);
    }
    public interface IModelAttribute
    {
        string[] ViewModelKeys { get; set; }
        bool IsMatch(IViewModelData data, IViewModelKeyProvider provider);
    }
    public interface IContentModelAttribute : IModelAttribute
    {
        string SchemaName { get; }
        bool InlineEditable { get; set; }
        bool IsDefault { get; }
    }
    public interface IPageModelAttribute : IModelAttribute
    {
        //What Properties go here to identify a Page Model?
    }
}
