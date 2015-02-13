using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mock = DD4T.ViewModels;
using DVM4T.DD4T;
using DVM4T.Testing.Models;
using Dynamic = DD4T.ContentModel;
using System.Web;
using Ploeh.AutoFixture;
using DD4T.Mvc.SiteEdit;
using System.IO;
using DVM4T.Contracts;
using System.Reflection;
using DVM4T.XPM;
using System.Web.Mvc;
using System.Linq.Expressions;
using MockContracts = DD4T.ViewModels.Contracts;
using MockModels = Mocking.DVM4T.Testing.MockModels;
using DVM4T.DD4T.XPM;
using DVM4T.Core;
using DVM4T.Exceptions;

namespace DVM4T.Testing
{
    //TODO: Clean up the tests, make them only test 1 thing at a time
    //TODO: Create multiple Tests files, this one is too long

    
    //TODO: Remove or change the DD4T.ViewModels.Mocking stuff - it's super confusing

    [TestClass]
    public class Tests
    {
        private Random r = new Random();
        private Fixture autoMocker = new Fixture();
        [TestInitialize]
        public void Init()
        {
            //Mock up Current HttpContext so we can use Site Edit
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://ei8htSe7en.io", "num=" + GetRandom()),
                new HttpResponse(new StringWriter())
                ) { };
            MockSiteEdit("1", true);
            //XpmUtility.XpmMarkupService = new XpmMarkupService();
        }
        private void MockSiteEdit(string pubId, bool enabled)
        {
            //Mock up Site Edit Settings for Pub ID 1
            SiteEditService.SiteEditSettings.Enabled = enabled;
            if (!SiteEditService.SiteEditSettings.ContainsKey(pubId))
            {
                SiteEditService.SiteEditSettings.Add(pubId,
                   new SiteEditSetting
                   {
                       Enabled = enabled,
                       ComponentPublication = pubId,
                       ContextPublication = pubId,
                       PagePublication = pubId,
                       PublishPublication = pubId
                   });
            }
        }
        private int GetRandom()
        {
            return r.Next(0, 101);
        }
        [TestMethod]
        public void TestBuildCPViewModelGeneric()
        {
            ContentContainerViewModel model = null;
            IViewModelBuilder builder = ViewModelDefaults.Builder;
            for (int i = 0; i < 10000; i++)
            {
                Dynamic.IComponentPresentation cp = GetMockCp(GetMockModel());
                model = builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            }
            Assert.IsNotNull(model); //Any better Assertions to make?
        }
        [TestMethod]
        public void TestBuildCPViewModelLoadedAssemblies()
        {

            ContentContainerViewModel model = null;
            IViewModelBuilder builder = ViewModelDefaults.Builder;
            for (int i = 0; i < 10000; i++)
            {
                Dynamic.IComponentPresentation cp = GetMockCp(GetMockModel());
                builder.LoadViewModels(Assembly.GetAssembly(typeof(ContentContainerViewModel)));
                model = (ContentContainerViewModel)builder.BuildCPViewModel(new ComponentPresentation(cp));
            }
            Assert.IsNotNull(model); //Any better Assertions to make?
        }

        [TestMethod]
        public void TestBasicXpmMarkup()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var titleMarkup = model.XpmMarkupFor(m => m.Title);
            Assert.IsNotNull(titleMarkup);
        }

        [TestMethod]
        public void TestLinkedComponentXpmMarkup()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var markup = ((GeneralContentViewModel)model.Content[0]).XpmMarkupFor(m => m.Body);
            Assert.IsNotNull(markup);
        }
        [TestMethod]
        public void TestEmbeddedXpmMarkup()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmMarkupFor(m => m.LinkText);
            Assert.IsNotNull(embeddedTest);
        }
        [TestMethod]
        public void TestXpmEditingZone()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var compMarkup = model.StartXpmEditingZone();
            Assert.IsNotNull(compMarkup);
        }

        [TestMethod]
        public void TestEditableField()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            HtmlString contentMarkup;
            foreach (var content in model.Content)
            {
                contentMarkup = model.XpmEditableField(m => m.Content, content);
            }
            var titleMarkup = model.XpmEditableField(m => m.Title);
            var compMarkup = model.StartXpmEditingZone();
            var markup = ((GeneralContentViewModel)model.Content[0]).XpmEditableField(m => m.Body);
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmEditableField(m => m.LinkText);
            Assert.IsNotNull(markup);
        }
        [TestMethod]
        public void GetMockCp()
        {
            var model = GetMockCp(GetMockModel());
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void TestBodyField()
        {
            string expectedString = autoMocker.Create<string>();
            var cp = GetCPMockup<MockModels.GeneralContentViewModel, MvcHtmlString>(x => x.Body, new MvcHtmlString(expectedString));
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<GeneralContentViewModel>(new ComponentPresentation(cp));
            Assert.AreEqual(expectedString, newModel.Body.ToHtmlString());
        }

        private Dynamic.ComponentPresentation GetManuallyBuiltCp()
        {
            Dynamic.Component comp = new Dynamic.Component { Id = "tcm:1-23", ComponentType = Dynamic.ComponentType.Normal };
            var linksFieldSet = new Dynamic.FieldSet 
                { 
                    {
                        "internalLink",
                        new Dynamic.Field { LinkedComponentValues = new List<Dynamic.Component> 
                            { 
                                comp,
                            },
                            FieldType = Dynamic.FieldType.ComponentLink
                        }
                    }
                };
            var links = new List<Dynamic.FieldSet>
            {
                linksFieldSet, linksFieldSet, linksFieldSet

            };
            var cp = new Dynamic.ComponentPresentation
            {
                Component = new Dynamic.Component
                {
                    Id = "tcm:1-45",
                    Fields = new Dynamic.FieldSet
                    {
                        {
                            "links", new Dynamic.Field
                            {
                                EmbeddedValues = links,
                                FieldType = Dynamic.FieldType.Embedded,
                                EmbeddedSchema = new Dynamic.Schema { Id = "tcm:1-354345-8", Title="EmbeddedLink" }
                            }
                        }
                    }

                },
                ComponentTemplate = new Dynamic.ComponentTemplate()
                {
                    MetadataFields = new Dynamic.FieldSet
                    {
                        { "view", new Dynamic.Field 
                            { 
                                Values = new List<string> { "Component View Name"},
                                FieldType = Dynamic.FieldType.Text
                            }
                        }
                    }
                }
            };
            return cp;
        }

        [TestMethod]
        public void TestEmbeddedAndIComponentField()
        {
            //setup
            string expectedString = autoMocker.Create<string>();
            var cp = GetManuallyBuiltCp();
            cp.Component.Id = expectedString;
            //exercise
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            //test
            Assert.AreEqual(3, newModel.Links.Count);
            Assert.AreEqual(cp.Component.Fields["links"].EmbeddedValues[0]["internalLink"].LinkedComponentValues[0].Id,
                newModel.Links.FirstOrDefault<EmbeddedLinkViewModel>().InternalLink.Id);
        }

        [TestMethod]
        public void TestLinkedComponentRichTextField()
        {
            string expectedString = autoMocker.Create<string>();
            var linkedCompModel = GetCPModelMockup<MockModels.GeneralContentViewModel, MvcHtmlString>(
                x => x.Body, new MvcHtmlString(expectedString));
            var cp = GetCPMockup<MockModels.ContentContainerViewModel, Mock.ViewModelList<MockModels.GeneralContentViewModel>>(
                x => x.Content, new Mock.ViewModelList<MockModels.GeneralContentViewModel> { linkedCompModel });
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            Assert.AreEqual(expectedString,
                newModel.Content.FirstOrDefault<GeneralContentViewModel>().Body.ToHtmlString());
        }

        [TestMethod]
        public void TestEmbeddedField()
        {
            string expectedString = autoMocker.Create<string>();
            var linkedCompModel = autoMocker.Build<MockModels.EmbeddedLinkViewModel>()
                 .Without(x => x.ComponentTemplate)
                 .Without(x => x.Builder)
                 .Without(x => x.Fields)
                 .Without(x => x.InternalLink)
                 .With(x => x.LinkText, expectedString)
                 .Create();
            var cp = GetCPMockup<MockModels.ContentContainerViewModel, Mock.ViewModelList<MockModels.EmbeddedLinkViewModel>>(
                x => x.Links, new Mock.ViewModelList<MockModels.EmbeddedLinkViewModel> { linkedCompModel });
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            Assert.AreEqual(expectedString,
                newModel.Links.FirstOrDefault<EmbeddedLinkViewModel>().LinkText);
        }

        [TestMethod]
        public void TestViewModelKey()
        {
            string viewModelKey = "TitleOnly";
            ViewModelDefaults.Builder.LoadViewModels(typeof(GeneralContentViewModel).Assembly);
            var cp = GetMockCp(GetMockModel());
            ((Dynamic.ComponentTemplate)cp.ComponentTemplate).MetadataFields = new Dynamic.FieldSet
            {
                {
                    "viewModelKey",
                    new Dynamic.Field { Values = new List<string> { viewModelKey }}
                }
            };
            ((Dynamic.Component)cp.Component).Title = "test title";

            //exercise
            var model = ViewModelDefaults.Builder.BuildCPViewModel(new ComponentPresentation(cp));

            //test
            Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }

        [TestMethod]
        public void TestCustomKeyProvider()
        {
            string key = autoMocker.Create<string>();
            var cp = GetManuallyBuiltCp();
            cp.ComponentTemplate.MetadataFields = new Dynamic.FieldSet()
            {
                {
                    key,
                    new Dynamic.Field { Values = new List<string> { "TitleOnly" }}
                }
            };
            cp.Component.Schema = new Dynamic.Schema { Title = "ContentContainer" };
            var provider = new CustomKeyProvider(key);
            var builder = new ViewModelBuilder(provider);
            builder.LoadViewModels(typeof(ContentContainerViewModel).Assembly);
            var model = builder.BuildCPViewModel(new ComponentPresentation(cp));

            Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }

        [TestMethod, ExpectedException(typeof(PropertyTypeMismatchException))]
        public void TestFieldTypeMismatch()
        {
            string expectedString = autoMocker.Create<string>();
            var cp = GetCPMockup<MockModels.GeneralContentViewModel, string>(
                x => x.Title, expectedString);
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<BrokenViewModel>(new ComponentPresentation(cp)); //should throw exception
        }

        [TestMethod]
        public void TestOOXPM()
        {
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(
                new ComponentPresentation(GetMockCp(GetMockModel())));
            //make sure the nested component gets a mock ID - used to test if site edit is enabled for component
            var generalContentXpm = new XpmRenderer<GeneralContentViewModel>((GeneralContentViewModel)model.Content[0], new XpmMarkupService());
            var markup = generalContentXpm.XpmMarkupFor(m => m.Body);
            Assert.IsNotNull(markup);
        }


        private TModel GetCPModelMockup<TModel, TProp>(Expression<Func<TModel, TProp>> propLambda, TProp value)
            where TModel : MockContracts.IComponentPresentationViewModel
        {
            return autoMocker.Build<TModel>()
                 .Without(x => x.ComponentPresentation)
                 .Without(x => x.Builder)
                 .Without(x => x.Fields)
                 .With(propLambda, value)
                 .Create();
        }
        private TModel GetEmbeddedModelMockup<TModel, TProp>(Expression<Func<TModel, TProp>> propLambda, TProp value)
            where TModel : MockContracts.IEmbeddedSchemaViewModel
        {
            return autoMocker.Build<TModel>()
                 .Without(x => x.ComponentTemplate)
                 .Without(x => x.Builder)
                 .Without(x => x.Fields)
                 .With(propLambda, value)
                 .Create();
        }


        private Dynamic.IComponentPresentation GetCPMockup<TModel, TProp>(Expression<Func<TModel, TProp>> propLambda, TProp value)
            where TModel : MockContracts.IComponentPresentationViewModel
        {
            return Mock.ViewModelDefaults.Mocker.ConvertToComponentPresentation(GetCPModelMockup<TModel, TProp>(propLambda, value));
        }


        private MockContracts.IDD4TViewModel GetMockModel()
        {
            MockModels.ContentContainerViewModel model = new MockModels.ContentContainerViewModel
            {
                Content = new Mock.ViewModelList<MockModels.GeneralContentViewModel>
                {
                    new MockModels.GeneralContentViewModel
                    {
                        Body = new MvcHtmlString("<p>" + GetRandom() + "</p>"),
                        Title = "The title" + GetRandom(),
                        SubTitle = "The sub title"+ GetRandom(),
                        NumberFieldExample = GetRandom(),
                        ShowOnTop = true
                    }
                },
                Links = new Mock.ViewModelList<MockModels.EmbeddedLinkViewModel>
                {
                    new MockModels.EmbeddedLinkViewModel
                    {
                        LinkText = "I am a link " + GetRandom(),
                        ExternalLink = "http://google.com",
                        InternalLink = new Dynamic.Component { Id = GetRandom().ToString() },
                        Target = "_blank" + GetRandom()
                    }
                },
                Title = "I am a content container!"
            };
            return model;
        }

        private Dynamic.IComponentPresentation GetMockCp(MockContracts.IDD4TViewModel model)
        {
            Dynamic.IComponentPresentation cp = Mock.ViewModelDefaults.Mocker.ConvertToComponentPresentation(model);
            ((Dynamic.Component)cp.Component).Id = "tcm:1-23-16";
            return cp;
        }

    }

    public class CustomKeyProvider : ViewModelKeyProviderBase
    {
        public CustomKeyProvider(string fieldName)
        {
            this.ViewModelKeyField = fieldName;
        }
    }

}
