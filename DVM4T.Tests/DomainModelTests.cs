using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVM4T.Testing.DomainModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVM4T.DD4T;
using DVM4T.Core;
using DVM4T.DD4T.Attributes;

namespace DVM4T.Tests
{
    [TestClass]
    public class DomainModelTests
    {
        [TestMethod]
        public void TestMapping()
        {
            var cp = new DVM4T.Testing.Tests().GetContentContainerCp();
            var modelData = Dependencies.DataFactory.GetModelData(cp);
            var mapping = ViewModelDefaults.CreateModelMapping<ContentContainerViewModel>();
            mapping.AddMapping(x => x.Title, new TextFieldAttribute("title"));
            var model = ViewModelDefaults.Factory.BuildMappedModel(modelData, mapping);
            Assert.IsNotNull(model.Title);
        }
    }
}
