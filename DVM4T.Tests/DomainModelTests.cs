using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Testing.DomainModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVM4T.DD4T;
using DVM4T.Core;
using DVM4T.DD4T.Attributes;
using DVM4T.Contracts;
using DD4T.ContentModel;
using DVM4T.Core.Binding;

namespace DVM4T.Tests
{
    [TestClass]
    public class DomainModelTests
    {
        private IModelMapping contentContainerMapping;
        private IComponentPresentation contentContainerCp;
        private IComponentPresentationData contentContainerModelData;
        [TestInitialize]
        public void Init()
        {
            contentContainerMapping = ViewModelDefaults.CreateModelMapping<ContentContainerViewModel>();
            contentContainerCp = new DVM4T.Testing.Tests().GetContentContainerCp();
            contentContainerModelData = Dependencies.DataFactory.GetModelData(contentContainerCp);
        }
        [TestMethod]
        public void TestTextFieldMapping()
        {
            //contentContainerMapping.AddMapping(x => x.Title, new TextFieldAttribute("title"));
            var model = ViewModelDefaults.Factory.BuildMappedModel<ContentContainerViewModel>(contentContainerModelData, contentContainerMapping);
            Assert.IsNotNull(model.Title);
        }

        [TestMethod]
        public void TestNestedComponentMapping()
        {
            //var contentMapping = ViewModelDefaults.CreateModelMapping<GeneralContentViewModel>();

            //contentMapping.AddMapping(x => x.Title, new TextFieldAttribute("title"));
            var container = new BindingContainer(ViewModelDefaults.TypeResolver);
            container.Load(new TestModule(ViewModelDefaults.TypeResolver, ViewModelDefaults.ReflectionCache));
            var mapping = container.GetMapping<ContentContainerViewModel>();
            //colorMapping.AddMapping(x => x.RgbValue, new TextFieldAttribute("rgbValue"));
            //contentMapping.AddMapping(x => x.BackgroundColor, new KeywordModelAttribute<Color>("backgroundColor", colorMapping));
            //contentContainerMapping.AddMapping(x => x.ContentList,
            //    new LinkedModelAttribute<GeneralContentViewModel>("content", contentMapping)
            //    {
            //        AllowMultipleValues = true
            //    });

            var model = ViewModelDefaults.Factory.BuildMappedModel<ContentContainerViewModel>(contentContainerModelData, mapping);
            Assert.IsNotNull(model.ContentList);
        }
    }
}
