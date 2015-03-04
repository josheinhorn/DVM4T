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

namespace DVM4T.Tests
{
    [TestClass]
    public class DomainModelTests
    {
        private IModelMapping<ContentContainerViewModel> contentContainerMapping;
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
            contentContainerMapping.AddMapping(x => x.Title, new TextFieldAttribute("title"));
            var model = ViewModelDefaults.Factory.BuildMappedModel(contentContainerModelData, contentContainerMapping);
            Assert.IsNotNull(model.Title);
        }

        [TestMethod]
        public void TestNestedComponentMapping()
        {
            var contentMapping = ViewModelDefaults.CreateModelMapping<GeneralContentViewModel>();
            contentMapping.AddMapping(x => x.Title, new TextFieldAttribute("title"));
            var colorMapping = ViewModelDefaults.CreateModelMapping<Color>();
            colorMapping.AddMapping(x => x.RgbValue, new TextFieldAttribute("rgbValue"));
            contentMapping.AddMapping(x => x.BackgroundColor, new KeywordModelAttribute<Color>("backgroundColor", colorMapping));
            //Need to implement something like Dependency Injection (syntax inspired by Ninject):
            //MapModel<GeneralContentViewModel>().BindProperty(x => x.Title).ToAttribute(new TextFieldAttribute("title"));
            //or for nested objects:
            //MapModel<ContentContainerViewModel>().BindProperty(x => x.ContentList).ToMethod(
            //  context => new NestedModelFieldAttribute("content", context.Container.GetMapping<GeneralContentViewModel>) 
            //  { AllowMultipleValues = true }));
            //then retrieve with some sort of container:
            //IMappingContainer container = new MappingContainer();
            //container.Load(new MyMappingModule());
            //IModelMapping<GeneralContentViewModel> mapping = container.GetMapping<GeneralContentViewModel>();

            //Basically this needs to be more like Dependency Inject where Domain Models are bound to mappings
            //Otherwise we need a method for every single model type
            contentContainerMapping.AddMapping(x => x.ContentList,
                new LinkedModelAttribute<GeneralContentViewModel>("content", contentMapping)
                {
                    AllowMultipleValues = true
                });
            var model = ViewModelDefaults.Factory.BuildMappedModel(contentContainerModelData, contentContainerMapping);
            Assert.IsNotNull(model.ContentList);
        }
    }
}
