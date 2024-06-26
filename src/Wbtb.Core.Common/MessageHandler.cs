﻿namespace Wbtb.Core.Common
{
    public class MessageHandler
    {
        #region PROPERTIES

        /// <summary>
        /// Required. Plugin to use to send alert.
        /// </summary>
        public string Plugin { get;set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// If sending to a user, set unique id here. User object must contain config needed by plugin to send alert.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// If sending to a user, set unique id here. User object must contain config needed by plugin to send alert.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// If set, time mask for repeating alerts on breaking builds. Alerts will be sent only after time mask has elapsed since build break.
        /// </summary>
        public string Remind { get; set; }

        #endregion

        #region CTORS

        public MessageHandler()
        { 
            this.Enable = true;
        }

        #endregion
    }
}
