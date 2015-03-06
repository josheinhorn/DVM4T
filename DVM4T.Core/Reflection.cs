using DVM4T.Attributes;
using DVM4T.Contracts;
using DVM4T.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DVM4T.Reflection
{
    //public static class ReflectionUtility
    //{
    //    private static readonly ReflectionOptimizer reflectionCache = new ReflectionOptimizer();
    //    public static IReflectionHelper ReflectionCache { get { return reflectionCache; } }
    //    public static IViewModelResolver ModelResolver { get { return reflectionCache; } }
    //}

    public class DefaultViewModelResolver : IViewModelResolver
    {
        private Dictionary<Type, IList<IModelProperty>> modelProperties = new Dictionary<Type, IList<IModelProperty>>();
        //private Dictionary<Type, ViewModelAttribute> viewModelAttributes = new Dictionary<Type, ViewModelAttribute>();
        private Dictionary<Type, IModelAttribute> modelAttributes = new Dictionary<Type, IModelAttribute>();
        private readonly IReflectionHelper helper;
        public DefaultViewModelResolver(IReflectionHelper helper)
        {
            if (helper == null) throw new ArgumentNullException("helper");
            this.helper = helper;
        }

        #region IViewModelResolver
        public IList<IModelProperty> GetModelProperties(Type type)
        {
            IList<IModelProperty> result;
            if (!modelProperties.TryGetValue(type, out result))
            {
                lock (modelProperties)
                {
                    if (!modelProperties.TryGetValue(type, out result))
                    {
                        PropertyInfo[] props = type.GetProperties();
                        result = new List<IModelProperty>();
                        foreach (var prop in props)
                        {
                            var modelProp = BuildModelProperty(prop);
                            if (modelProp != null) result.Add(modelProp);
                        }
                        modelProperties.Add(type, result);
                    }
                }
            }
            return result;
        }
        public T GetCustomAttribute<T>(Type type) where T : IModelAttribute
        {
            IModelAttribute result;
            if (!modelAttributes.TryGetValue(type, out result))
            {
                lock (modelAttributes)
                {
                    if (!modelAttributes.TryGetValue(type, out result))
                    {
                        result = type.GetCustomAttributes(typeof(T), true).FirstOrDefault() as IModelAttribute;
                        modelAttributes.Add(type, result);
                    }
                }
            }
            return (T)result;
        }
        /// <summary>
        /// This implementation requires the View Model Type to have a public parameterless constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IViewModel ResolveModel(Type type, IViewModelData data)
        {
            //Use explicit cast or "as" cast? Using as will result in null return value if the Type passed in doesn't implement IViewModel
            //Using explicit cast (IViewModel) will result in InvalidCastException if Type doesn't implement IViewModel
            return (IViewModel)helper.CreateInstance(type);
        }
        public object ResolveModel(Type type)
        {
            return helper.CreateInstance(type);
        }
        public IModelProperty GetModelProperty(PropertyInfo propertyInfo)
        {
            IModelProperty result = null;
            IPropertyAttribute propAttribute = null;
            IList<IModelProperty> allModelProperties;
            if (modelProperties.TryGetValue(propertyInfo.DeclaringType, out allModelProperties))
            {
                result = FindOrBuildModelProperty(allModelProperties, propertyInfo);
            }
            else
            {
                lock (modelProperties)
                {
                    allModelProperties = GetModelProperties(propertyInfo.DeclaringType);
                    result = FindOrBuildModelProperty(allModelProperties, propertyInfo);
                }
            }
            
            return result;
        }

        public IModelProperty GetModelProperty<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return GetModelProperty<TSource,TProperty>(propertyLambda);
        }

        public IModelProperty GetModelProperty<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            if (propertyLambda == null) throw new ArgumentNullException("propertyLambda");
            return GetModelProperty(helper.GetPropertyInfo(propertyLambda));
        }

        public IModelProperty GetModelProperty(PropertyInfo propertyInfo, IPropertyAttribute attribute)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            //if (attribute == null) throw new ArgumentNullException("attribute");
            return new ModelProperty
            {
                Name = propertyInfo.Name,
                PropertyAttribute = attribute,
                Set = helper.BuildSetter(propertyInfo),
                Get = helper.BuildGetter(propertyInfo),
                PropertyType = propertyInfo.PropertyType
            };
        }

        public IModelProperty GetModelProperty<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda, IPropertyAttribute attribute)
        {
            return GetModelProperty(helper.GetPropertyInfo<TSource, TProperty>(propertyLambda), attribute);
        }

        public IReflectionHelper ReflectionHelper { get { return helper; } }

        public T ResolveModel<T>(params object[] ctorArgs)
        {
            return (T)ResolveModel(typeof(T));
        }
        #endregion

        #region Private methods

        private IModelProperty FindOrBuildModelProperty(IList<IModelProperty> allModelProperties, PropertyInfo propertyInfo)
        {
            IModelProperty result = null;
            result = allModelProperties.FirstOrDefault(x => x.Name == propertyInfo.Name);
            if (result == null)
            {
                result = BuildModelProperty(propertyInfo);
                allModelProperties.Add(result);
            }
            return result;
        }
        
        /// <summary>
        /// Builds a new Model Property object. Uses Reflection (GetCustomAttributes)
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private IModelProperty BuildModelProperty(PropertyInfo propertyInfo)
        {
            IModelProperty result = null;
            var propAttribute = propertyInfo.GetCustomAttributes(typeof(IPropertyAttribute), true).FirstOrDefault() as IPropertyAttribute;
            if (propAttribute != null) //only add properties that have the custom attribute
            {
                result = GetModelProperty(propertyInfo, propAttribute);
            }
            return result;
        }

        #endregion


        
    }

    public class ReflectionOptimizer : IReflectionHelper
    {
        private Dictionary<Type, List<IModelProperty>> modelProperties = new Dictionary<Type, List<IModelProperty>>();
        private Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();
        //private Dictionary<Type, ViewModelAttribute> viewModelAttributes = new Dictionary<Type, ViewModelAttribute>();
        private Dictionary<Type, IModelAttribute> modelAttributes = new Dictionary<Type, IModelAttribute>();
        private Dictionary<Type, Dictionary<string, Action<object, object>>> twoArgMethods = 
            new Dictionary<Type, Dictionary<string, Action<object, object>>>();


        public ReflectionOptimizer() { }

        public object CreateInstance(Type objectType)
        {
            Func<object> result = null;
            if (!constructors.TryGetValue(objectType, out result))
            {
                lock (constructors)
                {
                    if (!constructors.TryGetValue(objectType, out result))
                    {
                        DynamicMethod dynamicMethod =
                                new DynamicMethod("Create_" + objectType.Name,
                           objectType, new Type[0]);
                        // Get the default constructor of the plugin type
                        ConstructorInfo ctor = objectType.GetConstructor(new Type[0]);

                        // Generate the intermediate language.       
                        ILGenerator ilgen = dynamicMethod.GetILGenerator();
                        ilgen.Emit(OpCodes.Newobj, ctor);
                        ilgen.Emit(OpCodes.Ret);

                        // Create new delegate and store it in the dictionary
                        result = (Func<object>)dynamicMethod
                            .CreateDelegate(typeof(Func<object>));
                        constructors.Add(objectType, result);
                    }
                }
            }
            return result.Invoke();
        }
        public T CreateInstance<T>() where T : class, new()
        {
            return CreateInstance(typeof(T)) as T;
        }

        public Action<object, object> BuildSetter(PropertyInfo propertyInfo)
        {
            //Equivalent to:
            /*delegate (object i, object a)
            {
                ((DeclaringType)i).Property = (PropertyType)a;
            }*/
            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");
            var setterCall = Expression.Call(
                                Expression.Convert(instance, propertyInfo.DeclaringType), propertyInfo.GetSetMethod(),
                                Expression.Convert(argument, propertyInfo.PropertyType));
            return Expression.Lambda<Action<object, object>>(setterCall, instance, argument).Compile();
        }
        public Func<object, object> BuildGetter(PropertyInfo propertyInfo)
        {
            //Equivalent to:
            /*delegate (object obj)
            {
                return (object)((DeclaringType)obj).Property
            }*/
            ParameterExpression obj = Expression.Parameter(typeof(object), "obj");
            var getterCall = Expression.Convert(
                                Expression.Call(
                                    Expression.Convert(obj, propertyInfo.DeclaringType), propertyInfo.GetGetMethod()),
                            typeof(object));
            return Expression.Lambda<Func<object, object>>(getterCall, obj).Compile();
        }
        public PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            //Type type = typeof(TSource);
            if (propertyLambda == null) throw new ArgumentNullException("propertyLambda");
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            //Commented this out because it uses reflection
            //if (type != propInfo.ReflectedType &&
            //    !type.IsSubclassOf(propInfo.ReflectedType))
            //    throw new ArgumentException(string.Format(
            //        "Expression '{0}' refers to a property that is not from type {1}.",
            //        propertyLambda.ToString(),
            //        type));

            return propInfo;
        }
        public PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return GetPropertyInfo<TSource, TProperty>(propertyLambda);
        }

        public Action<object, object> BuildAddMethod<TCollection>()
        {
            return BuildAddMethod(typeof(TCollection));
        }
        public Action<object, object> BuildAddMethod(Type collectionType)
        {
            //Stuff for lists
            string methodName = "Add";
            Action<object, object> result = null;
            Dictionary<string, Action<object, object>> typeMethods;
            if (!twoArgMethods.TryGetValue(collectionType, out typeMethods))
            {
                typeMethods = new Dictionary<string,Action<object,object>>();
                twoArgMethods.Add(collectionType, typeMethods);
            }
            if (!typeMethods.TryGetValue(methodName, out result))
            {
                Type genericType = collectionType.GetGenericArguments()[0];
                if (genericType != null
                    && typeof(ICollection<>).MakeGenericType(genericType).IsAssignableFrom(collectionType)) //It has a generic type param and it implements ICollection<T>
                {
                    //Equivalent to:
                    /*delegate (object c, object a)
                    {
                        ((CollectionType)c).Add((GenericType)a);
                    }*/
                    var collection = Expression.Parameter(typeof(object), "c");
                    var itemToAdd = Expression.Parameter(typeof(object), "a");
                    MethodInfo addMethod = collectionType.GetMethod(methodName);
                    var addCall = Expression.Call(
                                        Expression.Convert(collection, collectionType), addMethod,
                                        Expression.Convert(itemToAdd, genericType));
                    result = Expression.Lambda<Action<object, object>>(addCall, collection, itemToAdd).Compile();
                    typeMethods.Add(methodName, result); //cache the result so we don't need to repeat this process
                }
                else throw new ArgumentException("The type must implement ICollection<" + genericType.Name + ">", "collectionType");
            }
            return result;
        }
    }
}
