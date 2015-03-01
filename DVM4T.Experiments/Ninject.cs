using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Injection;
using Ninject.Components;
using Ninject.Modules;
using DVM4T.Contracts;
using DVM4T.Core;
using DVM4T.Reflection;
using DVM4T.DD4T;
using Ninject.Parameters;

namespace DVM4T.Experiments
{
    public class DD4TImplementationModule : NinjectModule
    {
        public override void Load()
        {
            //IViewModelKeyProvider keyProvider, IViewModelResolver resolver)
            Bind<IViewModelKeyProvider>().To<WebConfigViewModelKeyProvider>()
                .WithConstructorArgument("webConfigKey", 
                context => context.Parameters.FirstOrDefault(x => x.Name == "webConfigKey").GetValue(context, null));
                //.WithConstructorArgument<string>("DVM4T.ViewModelKeyMetaFieldName");
            Bind<IReflectionHelper>().To<ReflectionOptimizer>().InSingletonScope();
            Bind<IViewModelResolver>().To<DefaultViewModelResolver>().InSingletonScope();
            Bind<IViewModelFactory>().To<ViewModelFactory>().InSingletonScope();
            Bind<IModelDataFactory>().To<DefaultModelDataFactory>().InSingletonScope();
            Bind<IDD4TFactory>().To<DefaultDD4TFactory>().InSingletonScope();
        }
    }
    public static class CompositionRoot
    {
        private readonly static IKernel kernel = new StandardKernel(new DD4TImplementationModule());
        
        public static T Get<T>() 
        {
            return kernel.Get<T>();
        }
        public static IEnumerable<T> GetAll<T>()
        {
            return kernel.GetAll<T>();
        }
        public static IViewModelFactory GetModelFactory(string keyProviderSettingKey)
        {
            return kernel.Get<IViewModelFactory>(new Parameter("webConfigKey", keyProviderSettingKey, true));
        }
        public static IViewModelKeyProvider GetKeyProvider(string key)
        {
            return kernel.Get<IViewModelKeyProvider>(new Parameter("webConfigKey", key, true)); //seems like passing parameters is coupling to the implementation
        }


    }
}
