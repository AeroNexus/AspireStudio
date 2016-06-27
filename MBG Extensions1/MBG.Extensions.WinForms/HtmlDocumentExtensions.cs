﻿using System.IO;
using System.Linq;
using System.Windows.Forms;
using mshtml;

namespace MBG.Extensions.WinForms
{
    public static class HtmlDocumentExtensions
    {
        /// <summary>
        /// Adds the specified javascript to the HtmlDocument
        /// </summary>
        /// <param name="htmlDocument">The HtmlDocument</param>
        /// <param name="javaScript"></param>
        public static void AddScript(this HtmlDocument htmlDocument, string javaScript)
        {
            HtmlElement head = htmlDocument.GetElementsByTagName("head")[0];
            HtmlElement scriptElement = htmlDocument.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptElement.DomElement;
            element.text = javaScript;
            head.AppendChild(scriptElement);
        }

        public static void AddCSS(this HtmlDocument htmlDocument, string cssFileName)
        {
            IHTMLDocument2 currentDocument = (IHTMLDocument2)htmlDocument.DomDocument;
            int length = currentDocument.styleSheets.length;
            IHTMLStyleSheet styleSheet = currentDocument.createStyleSheet(@"", length + 1);
            using (TextReader reader = new StreamReader(cssFileName))
            {
                styleSheet.cssText = reader.ReadToEnd();
            }
        }

        public static void DoPostBack(this HtmlDocument document)
        {
            document.InvokeScript("__doPostBack");
        }
        public static void DoPostBack(this HtmlDocument document, string eventTarget, string eventArgument)
        {
            document.InvokeScript("__doPostBack", new object[] { eventTarget, eventArgument });
        }
        public static HtmlElement GetElementByTitle(this HtmlDocument document, string title)
        {
            return (from x in document.All.Cast<HtmlElement>()
                    where x.GetAttribute("title") == title
                    select x).SingleOrDefault();
        }
    }
}