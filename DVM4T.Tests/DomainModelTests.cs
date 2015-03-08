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
        private IComponentPresentation contentContainerCp;
        private IComponentPresentationData contentContainerModelData;
        [TestInitialize]
        public void Init()
        {
            contentContainerCp = new DVM4T.Testing.Tests().GetContentContainerCp();
            contentContainerModelData = Dependencies.DataFactory.GetModelData(contentContainerCp);
        }
        [TestMethod]
        public void TestTextFieldMapping()
        {
            //contentContainerMapping.AddMapping(x => x.Title, new TextFieldAttribute("title"));
            var container = new BindingContainer(ViewModelDefaults.ModelResolver, ViewModelDefaults.ReflectionCache,
                new TestModule());
            var mapping = container.GetMapping<ContentContainerViewModel>();
            var model = DefaultMappedModelFactory.Instance.BuildMappedModel<ContentContainerViewModel>(contentContainerModelData, mapping);
            Assert.IsNotNull(model.Title);
        }

        [TestMethod]
        public void TestNestedComponentMapping()
        {
            var container = new BindingContainer(ViewModelDefaults.ModelResolver, ViewModelDefaults.ReflectionCache,
                new TestModule());
            ContentContainerViewModel model = null;
            for (int i = 0; i < 1000; i++)
            {
                var mapping = container.GetMapping<ContentContainerViewModel>();
                model = DefaultMappedModelFactory.Instance.BuildMappedModel<ContentContainerViewModel>(contentContainerModelData, mapping);
            }
            Assert.IsNotNull(model.ContentList);
        }
    }
}
