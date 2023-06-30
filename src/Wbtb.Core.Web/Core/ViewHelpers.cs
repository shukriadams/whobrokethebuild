using Microsoft.AspNetCore.Html;
using System;
using System.Web;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public static class ViewHelpers
    {
        public static HtmlString PreserveLineBreaks(string content)
        {
            if (content == null)
                content = string.Empty;

            content = content
                .Replace("\r\n", "<br/>")
                .Replace("\r", "<br/>");

            return new HtmlString(content);
        }

        public static HtmlString BuildDuration(Build build) 
        {
            if (!build.EndedUtc.HasValue)
                return new HtmlString(string.Empty);

            return new HtmlString((build.EndedUtc.Value - build.StartedUtc).ToHumanString());
        }

        public static HtmlString StyleImageUrl(string url) 
        {
            if (string.IsNullOrEmpty(url))
                return new HtmlString("");

            return new HtmlString("style=\"background-image: url(" + url +")\"");
        }

        /// <summary>
        /// Converts build status to siumpler css classes. This is a soft conversation - it can handle null builds for less logic in view, and returns empty string in that case.
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public static string BuildStatusToCSSClass(Build build)
        {
            if (build == null)
                return string.Empty;

            if (build.Status == BuildStatus.Failed)
                return "broken";

            if (build.Status == BuildStatus.Passed)
                return "passing";
            
            return "build";
        }
        
        /// <summary>
        /// Generates best possible user page link from build involveement. Involvement can contain no user info at all, a user string name from source control which isn't directly 
        /// associated with a known user, or a mapped user that is known.
        /// </summary>
        /// <param name="involvement"></param>
        /// <returns></returns>
        public static HtmlString BuildInvolvementUserLink(BuildInvolvement involvement)
        { 
            if (involvement == null)
                return new HtmlString(string.Empty);

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!string.IsNullOrEmpty(involvement.MappedUserId)){
                IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
                User user = dataLayer.GetUserById(involvement.MappedUserId);
                return new HtmlString($"<a href=\"/user/{involvement.MappedUserId}\">{user.Name}</a>");
            }

            return new HtmlString($"<span class=\"\">User not resolved</span>");
        }

        public static HtmlString BuildInvolvementUserAvatar(BuildInvolvement involvement) 
        {
            if (involvement == null)
                return new HtmlString(string.Empty);

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!string.IsNullOrEmpty(involvement.MappedUserId))
            {
                IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
                User user = dataLayer.GetUserById(involvement.MappedUserId);
                string userImageUrl = user.Image;
                return new HtmlString($"<x-avatar class=\"--round\"><a title=\"{user.Name}\" href=\"/user/{involvement.MappedUserId}\"><img src=\"{userImageUrl}\" /></a></x-avatar>");
            }

            return new HtmlString($"<x-avatar class=\"--round --disabled\"><img /></x-avatar>");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="involvement"></param>
        /// <returns></returns>
        public static HtmlString BuildInvolvementUserLink(ViewBuildInvolvement involvement)
        {
            if (involvement == null)
                return new HtmlString("-");

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!string.IsNullOrEmpty(involvement.MappedUserId))
            {
                IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
                User user = dataLayer.GetUserById(involvement.MappedUserId);
                return new HtmlString($"<a href=\"/user/{involvement.MappedUserId}\">{user.Name}</a>");
            }

            if (involvement.Revision != null) 
                return new HtmlString($"<span class=\"\">{involvement.Revision.User} (unresolved)</span>");

            return new HtmlString($"<span class=\"\">(Revision unresolved).</span>");
        }

        
        public static HtmlString IncidentLink(Build build, string text = null)
        {
            if (text == null)
                text = build.Id; 

            if (build == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/incident/{build.Id}\">{text}</a>");
        }

        public static HtmlString BuildLink(Build build)
        { 
            if (build == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/build/{build.Id}\">{build.Identifier}</a>");
        }

        public static HtmlString JobLink(Job job)
        {
            if (job == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/job/{job.Id}\">{job.Name}</a>");
        }

        public static HtmlString BuildHostLink(Build build)
        {
            if (build == null)
                return new HtmlString(string.Empty);

            return new HtmlString($"<a href=\"/buildhost/{HttpUtility.UrlEncode(build.Hostname)}\">{build.Hostname}</a>");
        }


        /// <summary>
        /// Shorthand to force html string something
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static HtmlString String(string html)
        {
            return new HtmlString(html);
        }


        /// <summary>
        /// Translates build delta into a user-friendly string that clearly communicates status of build without assuming deep technical knowledge.
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public static string TranslateBuildStatus(Build build)
        { 
            // first try to use build delta 
            string result = string.Empty;

            if (result == string.Empty)
                switch(build.Status)
                { 
                    case BuildStatus.Failed:
                        result = "Broken";
                        break;

                    case BuildStatus.Passed:
                        result = "Passing";
                        break;

                    case BuildStatus.InProgress:
                        result = "Building";
                        break;

                }

            if (result == string.Empty)
                result = "Unknown";

            return result;
        }

        public static string GistOf(string text, int length, string overflow = "...")
        { 
            if (text.Length < length + overflow.Length)
                return text;
            
            return $"{text.Substring(0, length)}{overflow}";
        }

        public static HtmlString PagerBar<T>(string baseUrl, string queryStrings, PageableData<T> data, Configuration config)
        {
            Pager pager = new Pager();
            return new HtmlString(pager.Render(data, config.PagesPerPageGroup, baseUrl, queryStrings, "page"));
        }

        public static string TranslateBlame(BuildLogParseResult involvement)
        {
            switch(involvement.Blame)
            {
                case Blame.Innocent:
                    return "Did not break build";
                case Blame.Guilty:
                    return "Broke build";
                default:
                    return "Unknown if broke build";
            }
        }
    }
}
