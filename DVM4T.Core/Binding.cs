using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DVM4T.Core.Binding
{
     //BindModel<MyModel>()
     //   .FromProperty(x => x.MyProperty)
     //   .ToAttribute<MyFieldAttribute>("myField")
     //   .With(attr => 
     //       {
     //           attr.AllowMultipleValues = true;
     //           attr.IsMetadata = false;
     //           attr.InlineEditable = true;
     //       });
    public interface IModelMapping
    {
        Type ModelType { get; }
        IList<IModelProperty> ModelProperties { get; }
    }
    public interface ITypeResolver
    {
        T ResolveInstance<T>(params object[] ctorArgs);
        IModelProperty ResolveModelProperty(PropertyInfo propertyInfo, IPropertyAttribute attribute); //This method makes no sense here
    }
    public interface IPropertyMapping
    {
        PropertyInfo Property { get; }
        Action<IPropertyAttribute, IBindingContainer> GetMapping { get; set; }
        IPropertyAttribute PropertyAttribute { get; }
    }
    public interface IModelBinding<TModel>// where TModel : class
    {
        IPropertyBinding<TModel, TProp> FromProperty<TProp>(Expression<Func<TModel, TProp>> propertyLambda);
    }
    public interface IPropertyBinding<TModel, TProp>// where TModel : class
    {
        //void ToAttribute(IPropertyAttribute propertyAttribute);
        //void ToMethod(Func<IBindingContainer, IPropertyAttribute> attributeMethod);
        IAttributeBinding<TProp, TAttribute> ToAttribute<TAttribute>(params object[] ctorArguments) where TAttribute : IPropertyAttribute;
    }
    public interface IAttributeBinding<TProp, TAttribute> where TAttribute : IPropertyAttribute
    {
        void With(Action<TAttribute> action);
        //void WithMethod(Action<TAttribute, IBindingContainer> action); //Is this necessary? What are the use cases?
    }
    public interface IBindingContainer
    {
        IModelMapping GetMapping<T>(); // where T : class;
        IModelMapping GetMapping(Type type);
        void Load(IBindingModule module);
    }
    public interface IBindingModule //Responsible for loading bindings and turning them into useful model mapping
    {
        IModelBinding<T> BindModel<T>();
        void Load();
        void OnLoad(IBindingContainer container);
        IDictionary<Type, IList<IPropertyMapping>> ModelMappings { get; }
        //void AddBinding(); //Not supposed to pass binding -- what should be passed?
    }
   

}
