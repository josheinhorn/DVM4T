using DVM4T.Attributes;
using DVM4T.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DVM4T.Reflection
{
    public static class ReflectionUtility
    {
        private static readonly ReflectionOptimizer reflectionCache = new ReflectionOptimizer();
        public static IReflectionHelper ReflectionCache { get { return reflectionCache; } }
        public static IViewModelResolver ModelResolver { get { return reflectionCache; } }
    }

    public class ReflectionOptimizer : IReflectionHelper, IViewModelResolver
    {
        private Dictionary<Type, List<ModelAttributeProperty>> modelProperties = new Dictionary<Type, List<ModelAttributeProperty>>();
        private Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();
        //private Dictionary<Type, ViewModelAttribute> viewModelAttributes = new Dictionary<Type, ViewModelAttribute>();
        private Dictionary<Type, IModelAttribute> modelAttributes = new Dictionary<Type, IModelAttribute>();

        internal ReflectionOptimizer() { }

        public IList<ModelAttributeProperty> GetModelProperties(Type type)
        {
            List<ModelAttributeProperty> result;
            if (!modelProperties.TryGetValue(type, out result))
            {
                lock (modelProperties)
                {
                    if (!modelProperties.TryGetValue(type, out result))
                    {
                        PropertyInfo[] props = type.GetProperties();
                        result = new List<ModelAttributeProperty>();
                        foreach (var prop in props)
                        {
                            ModelPropertyAttributeBase propAttribute = prop.GetCustomAttributes(typeof(ModelPropertyAttributeBase), true).FirstOrDefault() as ModelPropertyAttributeBase;
                            if (propAttribute != null) //only add properties that have the custom attribute
                            {
                                result.Add(new ModelAttributeProperty
                                {
                                    Name = prop.Name,
                                    PropertyAttribute = propAttribute,
                                    Set = BuildSetter(prop),
                                    Get = BuildGetter(prop),
                                    PropertyType = prop.PropertyType
                                });
                            }
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

        /// <summary>
        /// This implementation requires the View Model Type to have a public parameterless constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IViewModel ResolveModel(Type type, IViewModelData data)
        {
            return this.CreateInstance(type) as IViewModel;
        }
    }


}
