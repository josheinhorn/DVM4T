using DVM4T.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DVM4T.Reflection
{
    public class ReflectionCache
    {
        private static Dictionary<Type, List<FieldAttributeProperty>> fieldProperties = new Dictionary<Type, List<FieldAttributeProperty>>();
        private static Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();
        private static Dictionary<Type, ViewModelAttribute> viewModelAttributes = new Dictionary<Type, ViewModelAttribute>();

        public static List<FieldAttributeProperty> GetFieldProperties(Type type)
        {
            List<FieldAttributeProperty> result;
            if (!fieldProperties.TryGetValue(type, out result))
            {
                lock (fieldProperties)
                {
                    if (!fieldProperties.TryGetValue(type, out result))
                    {
                        PropertyInfo[] props = type.GetProperties();
                        result = new List<FieldAttributeProperty>();
                        foreach (var prop in props)
                        {
                            FieldAttributeBase fieldAttribute = prop.GetCustomAttributes(typeof(FieldAttributeBase), true).FirstOrDefault() as FieldAttributeBase;
                            if (fieldAttribute != null) //only add properties that have the custom attribute
                            {
                                result.Add(new FieldAttributeProperty
                                {
                                    Name = prop.Name,
                                    FieldAttribute = fieldAttribute,
                                    Set = BuildSetter(prop),
                                    Get = BuildGetter(prop),
                                    PropertyType = prop.PropertyType
                                });
                            }
                        }
                        fieldProperties.Add(type, result);
                    }
                }
            }
            return result;
        }

        public static ViewModelAttribute GetViewModelAttribute(Type type)
        {
            ViewModelAttribute result;
            if (!viewModelAttributes.TryGetValue(type, out result))
            {
                lock (viewModelAttributes)
                {
                    if (!viewModelAttributes.TryGetValue(type, out result))
                    {
                        result = type.GetCustomAttributes(typeof(ViewModelAttribute), true).FirstOrDefault() as ViewModelAttribute;
                        viewModelAttributes.Add(type, result);
                    }
                }
            }
            return result;
        }

        public static object CreateInstance(Type objectType)
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
        public static T CreateInstance<T>() where T : class
        {
            return CreateInstance(typeof(T)) as T;   
        }

        private static Action<object, object> BuildSetter(PropertyInfo propertyInfo)
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
        private static Func<object, object> BuildGetter(PropertyInfo propertyInfo)
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
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
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
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            return GetPropertyInfo<TSource, TProperty>(propertyLambda);
        }
    }

    public struct FieldAttributeProperty
    {
        public string Name { get; set; }
        public Action<object, object> Set { get; set; }
        public Func<object, object> Get { get; set; }
        public FieldAttributeBase FieldAttribute { get; set; }
        public Type PropertyType { get; set; }
    }

}
