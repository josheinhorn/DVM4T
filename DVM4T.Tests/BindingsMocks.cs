using DVM4T.Contracts;
using DVM4T.Core;
using DVM4T.Core.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Testing.DomainModels;
using DVM4T.DD4T.Attributes;

namespace DVM4T.Tests
{
    public static class Bindings
    {
        public static readonly IBindingContainer Container;

        static Bindings()
        {
            Container = new BindingContainer(ViewModelDefaults.TypeResolver);
            Container.Load(new TestModule(ViewModelDefaults.TypeResolver, ViewModelDefaults.ReflectionCache));
        }
    }
    public class TestModule : BindingModuleBase
    {
        public TestModule(ITypeResolver resolver, IReflectionHelper helper) : base(resolver, helper)
        {
        }
        public override void Load()
        {
            BindModel<ContentContainerViewModel>()
                .FromProperty(m => m.Title)
                .ToAttribute<TextFieldAttribute>()
                .With(attr =>
                {
                    attr.FieldName = "title";
                });
            BindModel<ContentContainerViewModel>()
                .FromProperty(m => m.ViewName)
                .ToAttribute<TextFieldAttribute>()
                .With(attr =>
                {
                    attr.FieldName = "view";
                    attr.IsTemplateMetadata = true;
                });
            BindModel<ContentContainerViewModel>()
                .FromProperty(m => m.ContentList)
                .ToAttribute<LinkedComponentFieldAttribute>()
                .With(attr =>
                {
                    attr.FieldName = "content";
                    attr.AllowMultipleValues = true;
                });
            BindModel<GeneralContentViewModel>()
                .FromProperty(m => m.Title)
                .ToAttribute<TextFieldAttribute>()
                 .With(attr =>
                 {
                     attr.FieldName = "title";
                 });
        }

    }
}
