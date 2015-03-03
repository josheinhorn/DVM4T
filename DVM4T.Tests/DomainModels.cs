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

namespace DVM4T.Testing.DomainModels
{

    public class GeneralContentViewModel
    {
        public string Title { get; set; }

        public string SubTitle { get; set; }

        public MvcHtmlString Body { get; set; }

        public bool ShowOnTop { get; set; }

        public double NumberFieldExample { get; set; }

        public string ViewName { get; set; }

        public Color BackgroundColor { get; set; }

        public ListAdapter<StateType> State { get; set; }
    }

    public class ListAdapter<T> : List<object>, IEnumerable<T>
    {
        public new IEnumerator<T> GetEnumerator()
        {
            return this.ToArray().Cast<T>().GetEnumerator();
        }
    }

    public class Image
    {
        public string Url { get; set; }

        public IMultimediaData Multimedia { get; set; }

        public string AltText { get; set; }
    }

    public class ContentContainerViewModel
    {
        public string Title { get; set; }

        public ListAdapter<GeneralContentViewModel> ContentList { get; set; }

        public ListAdapter<EmbeddedLinkViewModel> Links { get; set; }

        public Image Image { get; set; }

        public string ViewName { get; set; }
    }

    public class EmbeddedLinkViewModel
    {
        public string LinkText { get; set; }

        public IComponent InternalLink { get; set; }

        public string ExternalLink { get; set; }

        public string Target { get; set; }
    }

    public class BrokenViewModel
    {
        public double BrokenTitle { get; set; }

        public string SubTitle { get; set; }

        public MvcHtmlString Body { get; set; }

        public bool ShowOnTop { get; set; }

        public double NumberFieldExample { get; set; }
    }

    public class Homepage
    {
        public ListAdapter<GeneralContentViewModel> Carousels { get; set; }

        public String Javascript { get; set; }
    }

    public class Color
    {
        public string Key { get; set; }

        public string RgbValue { get; set; }

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
