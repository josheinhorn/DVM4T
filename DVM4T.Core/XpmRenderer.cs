using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Contracts;
using System.Web;
using System.Linq.Expressions;
using System.Collections;
using DVM4T.Reflection;
using System.Reflection;

namespace DVM4T.XPM
{
    /// <summary>
    /// Renders XPM Markup for View Models
    /// </summary>
    /// <typeparam name="TModel">Type of the View Model</typeparam>
    public class XpmRenderer<TModel> : IXpmRenderer<TModel> where TModel : IContentViewModel
    {
        //This is just an OO implementation of the static extension methods... which one is better
        private IContentViewModel model;
        private IXpmMarkupService xpmMarkupService;
        public XpmRenderer(IContentViewModel model, IXpmMarkupService service)
        {
            this.model = model;
            this.xpmMarkupService = service;
        }
        /// <summary>
        /// Gets or sets the XPM Markup Service used to render the XPM Markup for the XPM extension methods
        /// </summary>
        public IXpmMarkupService XpmMarkupService
        {
            get { return xpmMarkupService; }
            set { xpmMarkupService = value; }
        }
        #region XPM Renderer methods
        /// <summary>
        /// Renders both XPM Markup and Field Value 
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="index">Optional index for a multi-value field</param>
        /// <returns>XPM Markup and field value</returns>
        public HtmlString XpmEditableField<TProp>(Expression<Func<TModel, TProp>> propertyLambda, int index = -1)
        {
            HtmlString result = null;
            var modelProp = GetModelProperty(propertyLambda);
            if (modelProp.PropertyAttribute is IFieldAttribute)
            {
                var fieldProp = modelProp.PropertyAttribute as IFieldAttribute;
                var fields = fieldProp.IsMetadata ? model.ModelData.Metadata : model.Content;
                result = SiteEditable<TProp>(model, fields, modelProp, index);
            }
            return result;
        }
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
        ///     @model.XpmEditableFieldField(m => m.Content, content);
        /// }
        /// </example>
        /// <returns>XPM Markup and field value</returns>
        public HtmlString XpmEditableField<TProp, TItem>(Expression<Func<TModel, TProp>> propertyLambda, TItem item)
        {
            HtmlString result = null;
            var modelProp = GetModelProperty(propertyLambda);
            if (modelProp.PropertyAttribute is IFieldAttribute)
            {
                var fieldProp = modelProp.PropertyAttribute as IFieldAttribute;
                var fields = fieldProp.IsMetadata ? model.ModelData.Metadata : model.Content;
                int index = IndexOf(modelProp, model, item);
                result = SiteEditable<TProp>(model, fields, modelProp, index);
            }
            return result;
        }
        /// <summary>
        /// Renders the XPM markup for a field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="index">Optional index for a multi-value field</param>
        /// <returns>XPM Markup</returns>
        public HtmlString XpmMarkupFor<TProp>(Expression<Func<TModel, TProp>> propertyLambda, int index = -1)
        {
            HtmlString result = null;
            if (IsSiteEditEnabled(model))
            {
                
                var modelProp = GetModelProperty(propertyLambda);
                if (modelProp.PropertyAttribute is IFieldAttribute)
                {
                    var fieldProp = modelProp.PropertyAttribute as IFieldAttribute;
                    var fields = fieldProp.IsMetadata ? model.ModelData.Metadata : model.Content;
                    result = XpmMarkupFor(fields, modelProp, index);
                }
            }
            return result;
        }
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
        public HtmlString XpmMarkupFor<TProp, TItem>(Expression<Func<TModel, TProp>> propertyLambda, TItem item)
        {
            HtmlString result = null;
            if (IsSiteEditEnabled(model))
            {

                var modelProp = GetModelProperty(propertyLambda);
                if (modelProp.PropertyAttribute is IFieldAttribute)
                {
                    var fieldProp = modelProp.PropertyAttribute as IFieldAttribute;
                    var fields = fieldProp.IsMetadata ? model.ModelData.Metadata : model.Content;
                    int index = IndexOf(modelProp, model, item);
                    result = XpmMarkupFor(fields, modelProp, index);
                }
            }
            return result;
        }
        /// <summary>
        /// Renders the XPM Markup for a Component Presentation
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="region">Region</param>
        /// <returns>XPM Markup</returns>
        public HtmlString StartXpmEditingZone(string region = null)
        {
            if (model is IComponentPresentationViewModel)
                return new HtmlString(XpmMarkupService.RenderXpmMarkupForComponent(((IComponentPresentationViewModel)model).ComponentPresentation, region));
            else return null;
        }
        #endregion

        #region private methods
        private bool IsSiteEditEnabled(IViewModel model)
        {
            return XpmMarkupService.IsSiteEditEnabled(model.ModelData.PublicationId);
        }

        private int IndexOf(IEnumerable enumerable, object obj)
        {
            if (obj != null)
            {
                int i = 0;
                foreach (var item in enumerable)
                {
                    if (item.Equals(obj)) return i;
                    i++;
                }
            }
            return -1;
        }
        private int IndexOf<T>(ModelAttributeProperty fieldProp, object model, T item)
        {
            int index = -1;
            object value = fieldProp.Get(model);
            if (value is IEnumerable<T>)
            {
                IEnumerable<T> list = (IEnumerable<T>)value;
                index = IndexOf(list, item);
            }
            else throw new FormatException(String.Format("Generic type of property type {0} does not match generic type of item {1}", value.GetType().Name, typeof(T).Name));
            return index;
        }
        private ModelAttributeProperty GetModelProperty<TProp>(Expression<Func<TModel, TProp>> propertyLambda)
        {
            PropertyInfo property = ReflectionUtility.ReflectionCache.GetPropertyInfo(propertyLambda);
            return GetModelProperty(typeof(TModel), property);
        }
        private HtmlString SiteEditable<TProp>(IViewModel model, IFieldsData fields, ModelAttributeProperty fieldProp, int index)
        {
            string markup = string.Empty;
            object value = null;
            string propValue = string.Empty;
            try
            {
                var field = GetField(fields, fieldProp);
                markup = IsSiteEditEnabled(model) ? GenerateSiteEditTag(field, index) : string.Empty;
                value = fieldProp.Get(model);
                propValue = value == null ? string.Empty : value.ToString();
            }
            catch (NullReferenceException)
            {
                return null;
            }
            return new HtmlString(markup + propValue);
        }

        private string GenerateSiteEditTag(IFieldData field, int index)
        {
            return XpmMarkupService.RenderXpmMarkupForField(field, index);
        }
        private IFieldData GetField(IFieldsData fields, ModelAttributeProperty fieldProp)
        {
            IFieldData result = null;
            if (fieldProp.PropertyAttribute is IFieldAttribute)
            {
                var fieldName = ((IFieldAttribute)fieldProp.PropertyAttribute).FieldName;
                result = fields.ContainsKey(fieldName) ? fields[fieldName] : null;
            }
            return result;
        }

        private ModelAttributeProperty GetModelProperty(Type type, PropertyInfo property)
        {
            var props = ReflectionUtility.ReflectionCache.GetModelProperties(type);
            return props.FirstOrDefault(x => x.Name == property.Name);
        }

        private HtmlString XpmMarkupFor(IFieldsData fields, ModelAttributeProperty fieldProp, int index)
        {
            try
            {
                return new HtmlString(GenerateSiteEditTag(GetField(fields, fieldProp), index));
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
        #endregion
    }
}
