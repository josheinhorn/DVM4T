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
using DVM4T.Reflection;
using DVM4T.Experiments;

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
            IViewModelFactory builder = ViewModelDefaults.Factory;
            for (int i = 0; i < 10000; i++)
            {
                Dynamic.IComponentPresentation cp = GetMockCp(GetMockModel());
                model = builder.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            }
            Assert.IsNotNull(model); //Any better Assertions to make?
        }
        [TestMethod]
        public void TestBuildCPViewModelLoadedAssemblies()
        {

            ContentContainerViewModel model = null;
            IViewModelFactory builder = ViewModelDefaults.Factory;
            for (int i = 0; i < 10000; i++)
            {
                Dynamic.IComponentPresentation cp = GetMockCp(GetMockModel());
                builder.LoadViewModels(Assembly.GetAssembly(typeof(ContentContainerViewModel)));
                model = (ContentContainerViewModel)builder.BuildViewModel(new ComponentPresentation(cp));
            }
            Assert.IsNotNull(model); //Any better Assertions to make?
        }

        [TestMethod]
        public void TestBasicXpmMarkup()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var titleMarkup = model.XpmMarkupFor(m => m.Title);
            Assert.IsNotNull(titleMarkup);
        }

        [TestMethod]
        public void TestLinkedComponentXpmMarkup()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var markup = ((GeneralContentViewModel)model.ContentList[0]).XpmMarkupFor(m => m.Body);
            Assert.IsNotNull(markup);
        }
        [TestMethod]
        public void TestEmbeddedXpmMarkup()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmMarkupFor(m => m.LinkText);
            Assert.IsNotNull(embeddedTest);
        }
        [TestMethod]
        public void TestXpmEditingZone()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            var compMarkup = model.StartXpmEditingZone();
            Assert.IsNotNull(compMarkup);
        }

        [TestMethod]
        public void TestEditableField()
        {
            var cp = GetMockCp(GetMockModel());
            ContentContainerViewModel model = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(
                new ComponentPresentation(cp));
            HtmlString contentMarkup;
            foreach (var content in model.ContentList)
            {
                contentMarkup = model.XpmEditableField(m => m.ContentList, content);
            }
            var titleMarkup = model.XpmEditableField(m => m.Title);
            var compMarkup = model.StartXpmEditingZone();
            var markup = ((GeneralContentViewModel)model.ContentList[0]).XpmEditableField(m => m.Body);
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
            var newModel = ViewModelDefaults.Factory.BuildViewModel<GeneralContentViewModel>(new ComponentPresentation(cp));
            Assert.AreEqual(expectedString, newModel.Body.ToHtmlString());
        }
        private Dynamic.Component GetGeneralContentComponent()
        {
            return new Dynamic.Component
            {
                Id = "tcm:1-22",
                ComponentType = Dynamic.ComponentType.Normal,
                Schema = new Dynamic.Schema
                {
                    Title = "GeneralContent"
                },
                Fields = new Dynamic.FieldSet
                {
                    {
                        "backgroundColor",
                        new Dynamic.Field
                        {
                            FieldType = Dynamic.FieldType.Keyword,
                            Keywords = new List<Dynamic.Keyword> { GetColorKeyword() },
                            CategoryName = "Colors"
                        }
                    },
                    {
                        "title",
                        new Dynamic.Field
                        {
                            FieldType = Dynamic.FieldType.Text,
                            Values = new List<string> { "I'm general content"}
                        }
                    },
                    {
                        "body",
                        new Dynamic.Field
                        {
                            FieldType = Dynamic.FieldType.Xhtml,
                            Values = new List<string> { "<p>Rich text</p>" }
                        }
                    },
                    {
                        "someNumber",
                        new Dynamic.Field
                        {
                            FieldType = Dynamic.FieldType.Number,
                            NumericValues = new List<double> { 234 }
                        }
                    }
                }
            };
        }
        private Dynamic.ComponentPresentation GetContentContainerCp()
        {
            Dynamic.Component comp = new Dynamic.Component { Id = "tcm:1-23", ComponentType = Dynamic.ComponentType.Normal };
            Dynamic.Component generalContent = GetGeneralContentComponent();

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
                    Schema = new Dynamic.Schema
                    {
                        Title = "ContentContainer"
                    },
                    Fields = new Dynamic.FieldSet
                    {
                        {
                            "links", 
                            new Dynamic.Field
                            {
                                EmbeddedValues = links,
                                FieldType = Dynamic.FieldType.Embedded,
                                EmbeddedSchema = new Dynamic.Schema { Id = "tcm:1-354345-8", Title="EmbeddedLink" }
                            }
                        },
                        {
                            "content",
                            new Dynamic.Field
                            {
                                FieldType = Dynamic.FieldType.ComponentLink,
                                LinkedComponentValues = new List<Dynamic.Component> { generalContent, generalContent }
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
            var cp = GetContentContainerCp();
            cp.Component.Id = expectedString;
            //exercise
            var newModel = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
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
            var newModel = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            Assert.AreEqual(expectedString,
                newModel.ContentList.FirstOrDefault<GeneralContentViewModel>().Body.ToHtmlString());
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
            var newModel = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(new ComponentPresentation(cp));
            Assert.AreEqual(expectedString,
                newModel.Links.FirstOrDefault<EmbeddedLinkViewModel>().LinkText);
        }

        [TestMethod]
        public void TestViewModelKey()
        {
            string viewModelKey = "TitleOnly";
            ViewModelDefaults.Factory.LoadViewModels(typeof(GeneralContentViewModel).Assembly);
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
            var model = ViewModelDefaults.Factory.BuildViewModel(new ComponentPresentation(cp));

            //test
            Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }

        [TestMethod]
        [ExpectedException(typeof(ViewModelTypeNotFoundException))]
        public void TestViewModelTypeNotFoundException()
        {
            string viewModelKey = "Non-existent View Model Key";
            ViewModelDefaults.Factory.LoadViewModels(typeof(GeneralContentViewModel).Assembly);
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
            var model = ViewModelDefaults.Factory.BuildViewModel(new ComponentPresentation(cp));

            //test
            //Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }
        [TestMethod]
        public void TestCustomKeyProvider()
        {
            string key = autoMocker.Create<string>();
            var cp = GetContentContainerCp();
            cp.ComponentTemplate.MetadataFields = new Dynamic.FieldSet()
            {
                {
                    key,
                    new Dynamic.Field { Values = new List<string> { "TitleOnly" }}
                }
            };
            cp.Component.Schema = new Dynamic.Schema { Title = "ContentContainer" };
            var provider = new CustomKeyProvider(key);
            var builder = new ViewModelFactory(provider, ViewModelDefaults.ModelResolver);
            builder.LoadViewModels(typeof(ContentContainerViewModel).Assembly);
            var model = builder.BuildViewModel(new ComponentPresentation(cp));

            Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }

        [TestMethod, ExpectedException(typeof(PropertyTypeMismatchException))]
        public void TestFieldTypeMismatch()
        {
            string expectedString = autoMocker.Create<string>();
            var cp = GetCPMockup<MockModels.GeneralContentViewModel, string>(
                x => x.Title, expectedString);
            var newModel = ViewModelDefaults.Factory.BuildViewModel<BrokenViewModel>(new ComponentPresentation(cp)); //should throw exception
        }

        [TestMethod]
        public void TestOOXPM()
        {
            ContentContainerViewModel model = ViewModelDefaults.Factory.BuildViewModel<ContentContainerViewModel>(
                new ComponentPresentation(GetMockCp(GetMockModel())));
            //make sure the nested component gets a mock ID - used to test if site edit is enabled for component
            var generalContentXpm = new XpmRenderer<GeneralContentViewModel>((GeneralContentViewModel)model.ContentList[0], new XpmMarkupService(), ViewModelDefaults.ModelResolver);
            var markup = generalContentXpm.XpmMarkupFor(m => m.Body);
            Assert.IsNotNull(markup);
        }

        [TestMethod]
        public void TestBuildPageModel()
        {
            string expectedString = autoMocker.Create<string>();
            var cp = GetCPMockup<MockModels.GeneralContentViewModel, MvcHtmlString>(x => x.Body, new MvcHtmlString(expectedString)) as Dynamic.ComponentPresentation;
            cp.ComponentTemplate = new Dynamic.ComponentTemplate
            {
                MetadataFields = new Dynamic.FieldSet
                {
                    {
                        "viewModelKey",
                        new Dynamic.Field { Values = new List<string> { "BasicGeneralContent"} }
                    },
                    {
                        "view",
                        new Dynamic.Field { Values = new List<string> { "CarouselGeneralContent"} }
                    }
                },
                Title = "Testing CT",
                Id = "tcm:1-123-32"
            };
            var page = new Dynamic.Page
            {
                PageTemplate = new Dynamic.PageTemplate
                {
                    MetadataFields = new Dynamic.FieldSet
                    {
                        { "viewModelKey",
                        new Dynamic.Field 
                            {
                                Values = new List<string> { "Homepage" },
                                Name = "viewModelKey",
                                FieldType = Dynamic.FieldType.Text,
                            }
                        }
                    }
                },
                MetadataFields = new Dynamic.FieldSet
                {
                     { "javascript",
                        new Dynamic.Field 
                            {
                                Values = new List<string> { "some javascript here" },
                                Name = "javascript",
                                FieldType = Dynamic.FieldType.Text,
                            }
                    }
                },
                ComponentPresentations = new List<Dynamic.ComponentPresentation>
                {
                    cp
                }
            };
            ViewModelDefaults.Factory.LoadViewModels(typeof(Homepage).Assembly);
            var pageModel = ViewModelDefaults.Factory.BuildViewModel(new Page(page));
            Assert.IsNotNull(pageModel);
        }

        [TestMethod]
        public void TestBuildKeywordModel()
        {
            Dynamic.IKeyword keyword = GetColorKeyword();
            var modelData = Dependencies.DataFactory.GetModelData(keyword, "Colors");
            ViewModelDefaults.Factory.LoadViewModels(this.GetType().Assembly);
            var model = ViewModelDefaults.Factory.BuildViewModel(modelData);
            Assert.IsInstanceOfType(model, typeof(Color));
        }

        [TestMethod]
        public void TestNestedKeyword()
        {
            object model = null;
            for (int i = 0; i < 100; i++)
            {
                var cp = GetContentContainerCp();
                var modelData = Dependencies.DataFactory.GetModelData(cp);
                ViewModelDefaults.Factory.LoadViewModels(this.GetType().Assembly);
                model = ViewModelDefaults.Factory.BuildViewModel(modelData);
            }
            Assert.IsInstanceOfType(model, typeof(ContentContainerViewModel));
        }

        [TestMethod]
        public void TestDependencyInjection()
        {
            object model = null;
            for (int i = 0; i < 100; i++) //Loop to test performance
            {
                var factory = CompositionRoot.GetModelFactory("DVM4T.ViewModelKeyMetaFieldName");
                factory.LoadViewModels(this.GetType().Assembly);
                var dataFactory = CompositionRoot.Get<IModelDataFactory>();
                var cp = GetContentContainerCp();
                var modelData = dataFactory.GetModelData(cp);
                model = factory.BuildViewModel(modelData);
            }
            Assert.IsInstanceOfType(model, typeof(ContentContainerViewModel));
        }
        private Dynamic.Keyword GetColorKeyword()
        {
            return new Dynamic.Keyword
            {
                Title = "Blue",
                Key = "blue",
                Id = "tcm:1-2-1024",
                MetadataFields = new Dynamic.FieldSet
                {
                    {
                        "rgbValue",
                        new Dynamic.Field
                        {
                            FieldType = Dynamic.FieldType.Text,
                            Values = new List<string> { "#0000FF"}
                        }
                    },
                    {
                        "decimalValue",
                        new Dynamic.Field
                        {
                            FieldType = Dynamic.FieldType.Number,
                            NumericValues = new List<double> { 255 }
                        }
                    }
                },
                TaxonomyId = "tcm:1-1-512"
            };
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
