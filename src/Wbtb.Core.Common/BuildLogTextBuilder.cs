using System.Text;

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
            _s.Append($"<x-logParse type='{type}'>");
        }

        public void AddItem(string content, string type = "") 
        {
            if (!_lineStarted)
                this.NewLine();

            _s.Append($"<x-logParseItem type='{type}'>{content}</x-logParseItem>");
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
