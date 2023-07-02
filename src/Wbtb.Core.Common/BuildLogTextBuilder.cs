using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Wbtb.Core.Common
{
    public class BuildLogTextBuilder
    {
        StringBuilder _s = new StringBuilder();

        private bool _closed = false;

        private bool _lineStarted = false;

        public BuildLogTextBuilder(string type = "") 
        {
            _closed = false;
            _s.Append($"<x-logParse type='{esc(type)}'>");
        }

        public BuildLogTextBuilder(PluginConfig parserPluginConfig)
        {
            _closed = false;
            _s.Append($"<x-logParse type='{esc(parserPluginConfig.Manifest.Key)}' version='{esc(parserPluginConfig.Manifest.Version)}'>");
        }

        public void AddItem(string content, string type = "") 
        {
            if (!_lineStarted)
                this.NewLine();

            _s.Append($"<x-logParseItem type='{esc(type)}'>{esc(content)}</x-logParseItem>");
        }

        private string esc(string s) 
        {
            // gee C#, all I want do is escape an xml string
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("tag");
            element.InnerText = s;
            return element.InnerXml;
        }
        public void NewLine() 
        {
            if (_lineStarted) 
            {
                _s.Append("</x-logParseLine>");
            }
            _s.Append("<x-logParseLine>");

            _lineStarted = true;
        }

        public string GetText()
        {
            if (_lineStarted)
                _s.Append("</x-logParseLine>");

            if (!_closed) 
            { 
                _s.Append("</x-logParse>");
                _closed = true;
            }

            return _s.ToString();
        }
    }
}
