﻿using DD4T.ContentModel;
using System;
using System.Web.Mvc;
using DVM4T.Attributes;
using DVM4T.Base;
using DVM4T.DD4T.Attributes;
using DVM4T.Core;

namespace DVM4T.Testing.Models
{
    [ViewModel("ContentContainer", false, ViewModelKeys = new string[] { "TitleOnly" })]
    public class TitleViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title")]
        public string Title { get; set; }
    }

    [ViewModel("GeneralContent", true, ViewModelKeys = new string[] { "BasicGeneralContent", "Test" })]
    public class GeneralContentViewModel : ComponentPresentationViewModelBase
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
    }

    [ViewModel("ContentContainer", true, ViewModelKeys = new string[] { "Test" })]
    public class ContentContainerViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title", InlineEditable = true)]
        public string Title { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
        public ViewModelList<GeneralContentViewModel> Content { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(TitleViewModel) })]
        public TitleViewModel OtherContent { get; set; }

        [EmbeddedSchemaField("links", typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public ViewModelList<EmbeddedLinkViewModel> Links { get; set; }
    }

    [ViewModel("EmbeddedLink", true)]
    public class EmbeddedLinkViewModel : EmbeddedSchemaViewModelBase
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
}