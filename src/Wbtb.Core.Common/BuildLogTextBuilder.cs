using System.Text;
using System.Xml;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Generates xml-like string that is saved in database by log log parsers. This xml contains strings a given parser can look for that can explain
    /// or describe build. This xml can then be picked up by post-processors and grouped to determine causes of build failures etc.
    /// 
    /// Note that the xml generated here is browser-safe and is also meant to be displayed as log summaries on the web ui.
    /// </summary>
    public class BuildLogTextBuilder
    {
        #region FIELDS

        StringBuilder _s = new StringBuilder();

        private bool _closed = false;

        private bool _lineStarted = false;

        #endregion

        #region CTORS

        public BuildLogTextBuilder(string type = "") 
        {
            _closed = false;
            _s.Append($"<x-logParse type='{EscapeXmlString(type)}'>");
        }

        public BuildLogTextBuilder(PluginConfig parserPluginConfig)
        {
            _closed = false;
            _s.Append($"<x-logParse");
            _s.Append($" type='{EscapeXmlString(parserPluginConfig.Manifest.Key)}' ");
            _s.Append($" key='{EscapeXmlString(parserPluginConfig.Key)}' ");
            _s.Append($" version='{EscapeXmlString(parserPluginConfig.Manifest.Version)}' ");
            _s.Append(">");

        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="type"></param>
        public void AddItem(string content, string type = "") 
        {
            if (!_lineStarted)
                this.NewLine();

            _s.Append($"<x-logParseItem type='{EscapeXmlString(type)}'>{EscapeXmlString(content)}</x-logParseItem>");
        }

        /// <summary>
        /// Escapes an xml string so it can be embedded in an xml string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string EscapeXmlString(string s) 
        {
            // gee C#, all I want do is escape an xml string
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement("tag");
            element.InnerText = s;
            return element.InnerXml;
        }

        /// <summary>
        /// 
        /// </summary>
        public void NewLine() 
        {
            if (_lineStarted) 
            {
                _s.Append("</x-logParseLine>");
            }
            _s.Append("<x-logParseLine>");

            _lineStarted = true;
        }

        /// <summary>
        /// Returns everything as a string. This is what is sent back to database for storage.
        /// </summary>
        /// <returns></returns>
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

        #endregion
    }
}
