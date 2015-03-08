using DD4T.ContentModel;
using System;
using System.Web.Mvc;
using DVM4T.Attributes;
using DVM4T.Base;
using DVM4T.DD4T.Attributes;
using DVM4T.Core;
using DVM4T.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace DVM4T.Testing.Models
{
    [ViewModel("ContentContainer", false, ViewModelKeys = new string[] { "TitleOnly" })]
    public class TitleViewModel : ViewModelBase
    {
        [TextField(FieldName = "title")]
        public string Title { get; set; }

        [ComponentTitle]
        public string CompTitle { get; set; }
    }

    [ViewModel("GeneralContent", true, ViewModelKeys = new string[] { "BasicGeneralContent", "Test" })]
    public class GeneralContentViewModel : ViewModelBase
    {
        [TextField(FieldName = "title")]
        public string Title { get; set; }

        [TextField(FieldName = "sutTitle", InlineEditable = true)]
        public string SubTitle { get; set; }

        [RichTextField(FieldName = "body", InlineEditable = true)]
        public MvcHtmlString Body { get; set; }

        [KeywordKeyField(FieldName = "showOnTop", IsBooleanValue = true)]
        public bool ShowOnTop { get; set; }

        [NumberField(FieldName = "someNumber")]
        public double NumberFieldExample { get; set; }

        [TextField(FieldName = "view", IsTemplateMetadata = true)]
        public string ViewName { get; set; }

        [KeywordField(FieldName = "backgroundColor", KeywordType = typeof(Color))]
        public Color BackgroundColor { get; set; }

        //[TextEnumField(FieldName = "state", AllowMultipleValues = true)]
        //public List<StateType> State { get; set; }
    }

    public class ListAdapter<T> : List<object>, IEnumerable<T> 
    {
        public new IEnumerator<T> GetEnumerator()
        {
            return this.ToArray().Cast<T>().GetEnumerator();
        }
    }

    [ViewModel("Image", true)]
    public class Image : ViewModelBase
    {
        [MultimediaUrl]
        public string Url { get; set; }

        [Multimedia]
        public IMultimediaData Multimedia { get; set; }

        [TextField(FieldName = "alt", IsMetadata = true)]
        public string AltText { get; set; }
    }
    [ViewModel("ContentContainer", true, ViewModelKeys = new string[] { "Test" })]
    public class ContentContainerViewModel : ViewModelBase
    {
        [TextField(FieldName = "title", InlineEditable = true)]
        public string Title { get; set; }

        [LinkedComponentField(FieldName = "content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
        public List<GeneralContentViewModel> ContentList { get; set; }

        [LinkedComponentField(FieldName = "content", LinkedComponentTypes = new Type[] { typeof(TitleViewModel) })]
        public TitleViewModel OtherContent { get; set; }

        [EmbeddedSchemaField(FieldName = "links", EmbeddedModelType = typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public List<EmbeddedLinkViewModel> Links { get; set; }

        [LinkedComponentField(FieldName = "image", LinkedComponentTypes = new Type[] { typeof(Image) })]
        public Image Image { get; set; }

        [TextField(FieldName = "view", IsTemplateMetadata = true)]
        public string ViewName { get; set; }
    }

    [ViewModel("EmbeddedLink", true)]
    public class EmbeddedLinkViewModel : ViewModelBase
    {
        [TextField(FieldName = "linkText")]
        public string LinkText { get; set; }

        [LinkedComponentField(FieldName = "internalLink")]
        public IComponent InternalLink { get; set; }

        //Requires a LinkProvider to work
        //[ResolvedUrlField(FieldName = "internalLink")]
        //public string ResolvedInternalLink { get; set; }

        [TextField(FieldName = "externalLink")]
        public string ExternalLink { get; set; }

        [KeywordKeyField(FieldName = "openInNewWindow", AllowMultipleValues = false)]
        public string Target { get; set; }
    }

    [ViewModel("GeneralContent", false, ViewModelKeys = new string[] { "Broken" })]
    public class BrokenViewModel : ViewModelBase
    {
        [TextField(FieldName = "title")]
        public double BrokenTitle { get; set; }

        [TextField(FieldName = "sutTitle", InlineEditable = true)]
        public string SubTitle { get; set; }

        [RichTextField(FieldName = "body", InlineEditable = true)]
        public MvcHtmlString Body { get; set; }

        [KeywordKeyField(FieldName = "showOnTop", IsBooleanValue = true)]
        public bool ShowOnTop { get; set; }

        [NumberField(FieldName = "someNumber")]
        public double NumberFieldExample { get; set; }

    }

    [PageViewModel(new string[] { "Homepage" })]
    public class Homepage : ViewModelBase
    {
        [PresentationsByView("Carousel")]
        public List<GeneralContentViewModel> Carousels { get; set; }

        [TextField(FieldName = "javascript", IsMetadata = true)]
        public String Javascript { get; set; }
    }

    [KeywordViewModel(new string[] {"Colors"})]
    public class Color : ViewModelBase
    {
        [KeywordData]
        public IKeywordData Keyword { get; set; }

        [TextField(FieldName = "rgbValue", IsMetadata = true)]
        public string RgbValue { get; set; }

        [NumberField(FieldName = "decimalValue", IsMetadata = true)]
        public double NumericValue { get; set; }
    }

    public enum StateType
    {
        VA,
        PA,
        NY,
        MA, 
        MD,
        DE
    }
}
