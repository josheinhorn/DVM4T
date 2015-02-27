using DD4T.ContentModel;
using System;
using System.Web.Mvc;
using DVM4T.Attributes;
using DVM4T.Base;
using DVM4T.DD4T.Attributes;
using DVM4T.Core;
using DVM4T.Contracts;
using System.Collections.Generic;

namespace DVM4T.Testing.Models
{
    [ViewModel("ContentContainer", false, ViewModelKeys = new string[] { "TitleOnly" })]
    public class TitleViewModel : ViewModelBase
    {
        [TextField("title")]
        public string Title { get; set; }

        [ComponentTitle]
        public string CompTitle { get; set; }
    }

    [ViewModel("GeneralContent", true, ViewModelKeys = new string[] { "BasicGeneralContent", "Test" })]
    public class GeneralContentViewModel : ViewModelBase
    {
        [TextField("title")]
        public string Title { get; set; }

        [TextField("sutTitle", InlineEditable = true)]
        public string SubTitle { get; set; }

        [RichTextField("body", InlineEditable = true)]
        public MvcHtmlString Body { get; set; }

        [KeywordKeyField("showOnTop", IsBooleanValue = true)]
        public bool ShowOnTop { get; set; }

        [NumberField("someNumber")]
        public double NumberFieldExample { get; set; }

        [TextField("view", IsTemplateMetadata = true)]
        public string ViewName { get; set; }

        [KeywordField("backgroundColor", KeywordType = typeof(Color))]
        public Color BackgroundColor { get; set; }
    }

    [ViewModel("Image", true)]
    public class Image : ViewModelBase
    {
        [MultimediaUrl]
        public string Url { get; set; }

        [Multimedia]
        public IMultimediaData Multimedia { get; set; }

        [TextField("alt", IsMetadata = true)]
        public string AltText { get; set; }
    }
    [ViewModel("ContentContainer", true, ViewModelKeys = new string[] { "Test" })]
    public class ContentContainerViewModel : ViewModelBase
    {
        [TextField("title", InlineEditable = true)]
        public string Title { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
        public ViewModelList<GeneralContentViewModel> ContentList { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(TitleViewModel) })]
        public TitleViewModel OtherContent { get; set; }

        [EmbeddedSchemaField("links", typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public ViewModelList<EmbeddedLinkViewModel> Links { get; set; }

        [LinkedComponentField("image", LinkedComponentTypes = new Type[] { typeof(Image) })]
        public Image Image { get; set; }

        [TextField("view", IsTemplateMetadata = true)]
        public string ViewName { get; set; }
    }

    [ViewModel("EmbeddedLink", true)]
    public class EmbeddedLinkViewModel : ViewModelBase
    {
        [TextField("linkText")]
        public string LinkText { get; set; }

        [LinkedComponentField("internalLink")]
        public IComponent InternalLink { get; set; }

        [TextField("externalLink")]
        public string ExternalLink { get; set; }

        [KeywordKeyField("openInNewWindow", AllowMultipleValues = false)]
        public string Target { get; set; }
    }

    [ViewModel("GeneralContent", false, ViewModelKeys = new string[] { "Broken" })]
    public class BrokenViewModel : ViewModelBase
    {
        [TextField("title")]
        public double BrokenTitle { get; set; }

        [TextField("sutTitle", InlineEditable = true)]
        public string SubTitle { get; set; }

        [RichTextField("body", InlineEditable = true)]
        public MvcHtmlString Body { get; set; }

        [KeywordKeyField("showOnTop", IsBooleanValue = true)]
        public bool ShowOnTop { get; set; }

        [NumberField("someNumber")]
        public double NumberFieldExample { get; set; }

    }

    [PageViewModel(new string[] { "Homepage" })]
    public class Homepage : ViewModelBase
    {
        [PresentationsByView("Carousel")]
        public ViewModelList<GeneralContentViewModel> Carousels { get; set; }

        [TextField("javascript", IsMetadata = true)]
        public String Javascript { get; set; }
    }

    [KeywordViewModel(new string[] {"Keyword"})]
    public class Color : ViewModelBase
    {
        [KeywordData]
        public IKeywordData Keyword { get; set; }

        [TextField("rgbValue", IsMetadata = true)]
        public string RgbValue { get; set; }

        [NumberField("decimalValue", IsMetadata = true)]
        public double NumericValue { get; set; }
    }
}
