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
            Container = new BindingContainer(ViewModelDefaults.ModelResolver, ViewModelDefaults.ReflectionCache,
                new TestModule());
        }
    }
    public class TestModule : BindingModuleBase
    {
        public override void Load()
        {
            var containerBinding = BindModel<ContentContainerViewModel>();
            containerBinding
                .FromProperty(m => m.Title)
                .ToAttribute<TextFieldAttribute>()
                .With(attr =>
                {
                    attr.FieldName = "title";
                });
            containerBinding
                .FromProperty(m => m.ViewName)
                .ToAttribute<TextFieldAttribute>()
                .With(attr =>
                {
                    attr.FieldName = "view";
                    attr.IsTemplateMetadata = true;
                });
            containerBinding
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
