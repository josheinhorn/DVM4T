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

namespace DVM4T.Experiments
{
    public class DefaultDependencyInjector : NinjectModule
    {
        public override void Load()
        {
            //IViewModelKeyProvider keyProvider, IViewModelResolver resolver)
            Bind<IViewModelKeyProvider>().To<WebConfigViewModelKeyProvider>()
                .WithConstructorArgument<string>("DVM4T.ViewModelKeyMetaFieldName");
            Bind<IReflectionHelper>().To<ReflectionOptimizer>().InSingletonScope();
            Bind<IViewModelResolver>().To<DefaultViewModelResolver>().InSingletonScope();
            Bind<IViewModelFactory>().To<ViewModelFactory>().InSingletonScope();
        }
    }


    public class CompositionRoot
    {
        private readonly static IKernel kernel = new StandardKernel(new DefaultDependencyInjector());
        
        public static T Get<T>() 
        {
            return kernel.Get<T>();
        }

    }
}
