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
    #region XPathable Data
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
    public interface IAttribute
    {
        string Key { get; set; }
        string Value { get; set; }
    }
    #endregion

    #region Tridion interfaces
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

    public interface IHaveNode
    {
        IXPathableNode Node { get; set; }
    }
    public interface ITridionItem
    {
        string TcmUri { get; }
        string Title { get; }
        int PublicationId { get; }
    }

    public interface IComponentPresentation
    {
        IComponent Component { get; }
        IComponentTemplate ComponentTemplate { get; }
    }

    public interface IComponent : ITridionItem
    {
        IFieldSet Fields { get; }
        IFieldSet MetadataFields { get; }
        ISchema Schema { get; }
    }
    
    public interface IComponentTemplate : ITridionItem
    {
        IFieldSet MetadataFields { get; }
        DateTime RevisionDate { get; }
    }

    public interface IFieldSet
    {
        IField this[string key] { get; } //Convert this to xpath
        bool ContainsKey(string key); //check if the XPath exists
    }

    public interface ISchema : ITridionItem
    {
        string RootElementName { get; }
    }
    public interface ICanUseXpm
    {
        string XPath { get; }
        string Name { get; }
        string RootElementName { get; }
    }
    public interface IField : ICanUseXpm, IHaveNode
    {
        //The object Value is the most powerful thing about this entire framework -- this object will be retrieved based on the FieldAttribute's overrided method. Question is, should it be implemented here.
        IEnumerable Value { get; } //can/should I do this?
        ISchema EmbeddedSchema { get; }
        string FieldTypeString { get; } //maybe make an enum?? meh
    }
    #endregion

    /// <summary>
    /// A DD4T View Model
    /// </summary>
    public interface IViewModel
    {
        IViewModelBuilder Builder { get; set; }
        IFieldSet Fields { get; set; } 
        IFieldSet MetadataFields { get; }
        IComponentTemplate ComponentTemplate { get; set; } //we required Component Template here in order to generate Site Edit markup for any linked components in the embedded fields
        /// <summary>
        /// Publication ID of the underlying Tridion item
        /// </summary>
        int PublicationId { get; }
    }


    public interface IComponentPresentationViewModel : IViewModel
    {
        IComponentPresentation ComponentPresentation { get; set; }
    }
    //TODO: Consider removing this interface, holding on to template is not actually necessary after building is done
    public interface IEmbeddedSchemaViewModel : IViewModel
    {
        
    }

    public interface IViewModelBuilder
    {
        /// <summary>
        /// Builds a View Model from previously loaded Assemblies using the input Component Presentation. This method infers the View Model type by comparing
        /// information from the Component Presentation object to the attributes of the View Model classes.
        /// </summary>
        /// <param name="cp">Component Presentation object</param>
        /// <remarks>
        /// The LoadViewModels method must be called with the desired View Model Types in order for this to return a valid object.
        /// </remarks>
        /// <returns>Component Presentation View Model</returns>
        IComponentPresentationViewModel BuildCPViewModel(IComponentPresentation componentPresentation); //A way to build view model without passing type -- type is inferred using loaded assemblies
        /// <summary>
        /// Builds a View Model from a FieldSet using the schema determine the View Model class to use.
        /// </summary>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="embeddedSchema">Embedded Schema</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        IEmbeddedSchemaViewModel BuildEmbeddedViewModel(IFieldSet embeddedFields, ISchema embeddedSchemaTitle, IComponentTemplate template);
        /// <summary>
        /// Builds a View Model from a Component Presentation using the type parameter to determine the View Model class to use.
        /// </summary>
        /// <param name="type">Type of View Model class to return</param>
        /// <param name="cp">Component Presentation</param>
        /// <returns>Component Presentation View Model</returns>
        IComponentPresentationViewModel BuildCPViewModel(Type type, IComponentPresentation componentPresentation);
        /// <summary>
        /// Builds a View Model from a FieldSet using the generic type to determine the View Model class to use.
        /// </summary>
        /// <param name="type">Type of View Model class to return</param>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        IEmbeddedSchemaViewModel BuildEmbeddedViewModel(Type type, IFieldSet embeddedFields, IComponentTemplate template);
        /// <summary>
        /// Builds a View Model from a Component Presentation using the generic type to determine the View Model class to use.
        /// </summary>
        /// <typeparam name="T">Type of View Model class to return</typeparam>
        /// <param name="cp">Component Presentation</param>
        /// <returns>Component Presentation View Model</returns>
        T BuildCPViewModel<T>(IComponentPresentation componentPresentation) where T : class, IComponentPresentationViewModel;
        /// <summary>
        /// Builds a View Model from a FieldSet using the generic type to determine the View Model class to use.
        /// </summary>
        /// <typeparam name="T">Type of View Model class to return</typeparam>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        T BuildEmbeddedViewModel<T>(IFieldSet embeddedFields, IComponentTemplate template) where T : class, IEmbeddedSchemaViewModel;
        /// <summary>
        /// The View Model Key Provider for this Builder
        /// </summary>
        IViewModelKeyProvider ViewModelKeyProvider { get; }
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
        /// <param name="field">DD4T Field</param>
        /// <param name="index">Optional index for multi value fields</param>
        /// <returns>XPM Markup</returns>
        string RenderXpmMarkupForField(IField field, int index = -1);
        /// <summary>
        /// Renders the XPM Markup for a Component Presentation
        /// </summary>
        /// <param name="cp">Component Presentation</param>
        /// <param name="region">Optional region</param>
        /// <returns>XPM Markup</returns>
        string RenderXpmMarkupForComponent(IComponentPresentation cp, string region = null);
        /// <summary>
        /// Determines if Site Edit is enabled for a particular item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns></returns>
        bool IsSiteEditEnabled(ITridionItem item);
        /// <summary>
        /// Determines if Site Edit is enabeld for a publication
        /// </summary>
        /// <param name="publicationId">Publication ID</param>
        /// <returns></returns>
        bool IsSiteEditEnabled(int publicationId);
    }

    //TODO: Instantiate and add these to the IDD4TViewModel using System.Reflection's MakeGenericType
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
        string GetViewModelKey(IComponentTemplate template);
    }

    public interface ICanBeBoolean
    {
        bool IsBooleanValue { get; set; }
    }
   
}
