﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// Single file, zero-dependency Perforce helper library.
/// </summary>
namespace Madscience.Perforce
{

    /// <summary>
    /// 
    /// </summary>
    public enum AnnotateChange 
    { 
        add,
        edit,
        delete
    }

    public class ClientView 
    {
        public string Remote { get; set; }
        public string Local { get; set; }
    }

    public class Client 
    {
        public string Name { get; set; }
        public string Root { get; set; }
        public IEnumerable<ClientView> Views { get; set; }

        public Client() 
        {
            this.Views = new ClientView[] { };
        } 
    }

    /// <summary>
    /// 
    /// </summary>
    public class Annotate
    { 
        /// <summary>
        /// Revision annotation was taken at
        /// </summary>
        public string Revision { get; set; }

        public string File { get; set; }

        public AnnotateChange Change {get;set; }

        public IList<AnnotateLine> Lines {get;set; }

        public Annotate()
        { 
            Lines = new List<AnnotateLine>();
            File= string.Empty;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnnotateLine
    {
        /// <summary>
        /// Revision that changed this line
        /// </summary>
        public string Revision {get;set; }

        public string Text { get; set; }
        
        /// <summary>
        /// Line number
        /// </summary>
        public int LineNumber { get; set; }

        public AnnotateLine()
        { 
            Text = string.Empty;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Change
    {
        public string Revision { get; set; }
        public string Workspace { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public string Description { get; set; }
        public int ChangeFilesCount { get; set; }
        public IEnumerable<ChangeFile> Files { get; set; }

        public Change()
        {
            Workspace = string.Empty;
            User = string.Empty;
            Description = string.Empty;
            Files = new List<ChangeFile>();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ChangeFile
    {
        public string File { get; set; }

        /// <summary>
        /// Can be (delete|add|edit|integrate)
        /// </summary>
        public string Change { get; set; }
        public IEnumerable<string> Differences { get; set; }

        public ChangeFile()
        {
            File = string.Empty;
            Change = string.Empty;
            Differences = new string[] { };
        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class ShellResult
    {
        public int ExitCode { get; set; }
        public IEnumerable<string> StdOut { get; set; }
        public IEnumerable<string> StdErr { get; set; }

        public ShellResult()
        {
            this.StdOut = new string[] { };
            this.StdErr = new string[] { };
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class PerforceUtils
    {
        public enum ShellType
        { 
            Sh, Cmd
        }

        private static Dictionary<string, string> _tickets = new Dictionary<string, string>();

        /// <summary>
        /// Runs a shell command synchronously, returns concatenated stdout, stderr and error code.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static ShellResult Run(string command)
        {
            Process cmd = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cmd.StartInfo.FileName = "sh";
                cmd.StartInfo.Arguments = $"-c \"{command}\"";
            }
            else 
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = $"/k {command}";
            }

            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            List<string> stdOut = new List<string>();
            List<string> stdErr = new List<string>();
            int timeout = 50000;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
            {
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    cmd.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            stdOut.Add(e.Data);
                    };

                    cmd.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            stdErr.Add(e.Data);
                    };

                    cmd.Start();
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || (cmd.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout)))
                        return new ShellResult
                        {
                            StdOut = stdOut,
                            StdErr = stdErr,
                            ExitCode = cmd.ExitCode
                        };
                    else
                        throw new Exception("Time out");
                }
            }
            else
            {
                cmd.Start();
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();


                while (!cmd.StandardOutput.EndOfStream)
                {
                    string line = cmd.StandardOutput.ReadLine();
                    stdOut.Add(line);
                }

                while (!cmd.StandardError.EndOfStream)
                {
                    string line = cmd.StandardError.ReadLine();
                    stdErr.Add(line);
                }

                return new ShellResult
                {
                    StdOut = stdOut,
                    StdErr = stdErr,
                    ExitCode = cmd.ExitCode
                };

            }

        }


        /// <summary>
        /// Converts windows line endings to unix
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string StandardizeLineEndings(string input)
        {
            return Regex.Replace(input, @"\r\n", "\n");
        }


        /// <summary>
        /// Compact and safe regex find - tries to find a string in another string, if found, returns result, else returns empty string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="regexPattern"></param>
        /// <returns></returns>
        private static string Find(string text, string regexPattern, RegexOptions options = RegexOptions.None, string defaultValue = "")
        {
            MatchCollection matches = new Regex(regexPattern, options).Matches(text);
            if (!matches.Any())
                return defaultValue;

            return string.Join(string.Empty, matches.Select(m => m.Groups[1].Value));
        }


        /// <summary>
        /// gets a p4 ticket for user. Tickets are cached in memory. This is a cludge fix for now, because ticket is generated once in memory, then we 
        /// assume it always works. If ticket is revoked after server start, we have to restart it to recreate ticket. tickets should be checked 
        /// on-the-fly, but that needs to be done higher up than this method.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="host"></param>
        private static string GetTicket(string username, string password, string host, string trustFingerPrint)
        {
            string key = username + host;
            if (_tickets.ContainsKey(key)) 
                return _tickets[key];

            string command = $"echo {password}|p4 -p {host} -u {username} login && p4 -p {host} tickets";
            if (!string.IsNullOrEmpty(trustFingerPrint))
                command = $"p4 -p {host} trust -i {trustFingerPrint.ToUpper()} && p4 -p {host} trust -f -y && {command}";

            var result = Run(command);
            // perforce cannot establish trust + get ticket at same time because it is stupid.
            if (string.Join("\n", result.StdOut).ToLower().Contains("already established"))
                result = Run(command);

            if (result.ExitCode != 0 || result.StdErr.Any())
                throw new Exception($"Failed to login, got code {result.ExitCode} - {string.Join("\n", result.StdErr)}");

            foreach (string outline in result.StdOut)
                if (outline.Contains($"({username})")) 
                {
                    string ticket = outline.Split(" ")[2];
                    _tickets.Add(key, ticket);
                    return ticket;
                }

            throw new Exception($"Failed to get ticket - {string.Join("\n", result.StdErr)} {string.Join("\n", result.StdOut)}. If trust is already established, ignore this error. ");
        }

        public static  bool IsInstalled() 
        {
            Console.Write("WBTB : Verifying p4 client available locally, you can safely ignore any authentication errors immediately following this line.");
            ShellResult result = Run($"p4");
            string stdErr = string.Join("", result.StdErr);
            if (stdErr.Contains("is not recognized as an internal or external command") || stdErr.Contains("'p4' not found"))
                return false;

            return true;
        }

        /// <summary>
        /// Gets detailed contents of a change using the P4 describe command. Returns null if revision does not exist.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="host"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        public static string GetRawDescribe(string username, string password, string host, string trustFingerPrint, int revision)
        {
            string ticket =  GetTicket(username, password, host, trustFingerPrint);
            string command = $"p4 -u {username} -p {host} -P {ticket} describe -s {revision}";

            ShellResult result = Run(command);

            if (result.ExitCode != 0 || result.StdErr.Any())
            { 
                string stderr = string.Join("\r\n", result.StdErr);
                if(stderr.Contains("no such changelist"))
                    return null;

                // ignore text encoding issues
                // todo : find a better way to fix this
                if (stderr.Contains("No Translation for parameter 'data'"))
                    throw new Exception("Invalid revision encoding"); // do not change exception message, we're hardcoded referring to further up

                if (stderr.Contains("'p4 trust' command"))
                    Console.WriteLine("Note that you can force p4 trust by adding Trust: true to your source server's Config: block");

                throw new Exception($"P4 command {command} exited with code {result.ExitCode}, revision {revision}, error : {stderr}");
            }

            return string.Join("\n", result.StdOut);
        }



        public static string GetRawClient(string username, string password, string host, string trustFingerPrint, string clientname) 
        {
            string ticket = GetTicket(username, password, host, trustFingerPrint);
            string command = $"p4 -u {username} -p {host} -P {ticket} client -o {clientname}";

            ShellResult result = Run(command);

            if (result.ExitCode != 0 || result.StdErr.Any())
            {
                string stderr = string.Join("\r\n", result.StdErr);

                // todo : how to detect invalid clientname ??

                if (stderr.Contains("'p4 trust' command"))
                    Console.WriteLine("Note that you can force p4 trust by adding Trust: true to your source server's Config: block");

                throw new Exception($"P4 command {command} exited with code {result.ExitCode}, error : {stderr}");
            }

            return string.Join("\n", result.StdOut);
        }

        public static Client ParseClient(string rawClient) 
        {
            /*
            
            Expected rawClient is :

                Client: myclient
                Access: 2020/04/12 10:18:31
                Owner:  myuser
                Host:   mypcname
                Description:
                        Created by myuser etc etc.
                Root:   D:\path\to\my\workspace
                Options:        noallwrite noclobber nocompress unlocked nomodtime normdir
                SubmitOptions:  submitunchanged
                LineEnd:        local
                Stream: //mydepot/mystream
                View:
                        //mydepot/mystream/mydir/%%1 //myclient/%%1
                        //mydepot/mystream/some/path/... //myclient/path/...
                        //mydepot/mystream/some/other/path/... //myclient/path2/...


            Note:
                first line in View remaps the files in mydir to the root directtory of the workspace
                
             */
            
            // convert all windows linebreaks to unix 
            rawClient = StandardizeLineEndings(rawClient);

            string[] commentStrip = rawClient.Split("\n");
            rawClient = string.Join("\n", commentStrip.Where(r => !r.StartsWith("#")));
            string name = Find(rawClient, @"Client:\s*(.*?)\n", RegexOptions.IgnoreCase);
            string root = Find(rawClient, @"Root:\s*(.*?)\n", RegexOptions.IgnoreCase);
            string viewItemsRaw = Find(rawClient, @"View:\n([\s\S]*.*?)", RegexOptions.Multiline);
            string[] viewItems = viewItemsRaw.Split("\n");
            IList <ClientView> views = new List<ClientView>();

            for (int i = 0; i < viewItems.Length; i++)
            {
                viewItems[i] = viewItems[i].Trim();
                // there will be empty string after trim, ignore these
                if (viewItems[i].Length == 0)
                    continue;

                views.Add(new ClientView { 
                    // everything before the space is the remote part of the view, everything after the local remap
                    Remote = Find(viewItems[i], @"(.*?)\s"),
                    Local = Find(viewItems[i], @"\s(.*?)$")
                });
            }

            return new Client 
            {
                Root = root,
                Name = name,
                Views = views
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="host"></param>
        public static void VerifyCredentials(string username, string password, string host, string trustFingerPrint)
        {
            GetTicket(username, password, host, trustFingerPrint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawDescribe"></param>
        /// <param name="parseDifferences"></param>
        /// <returns></returns>
        public static Change ParseDescribe(string rawDescribe, bool parseDifferences = true)
        {
            // convert all windows linebreaks to unix 
            rawDescribe = StandardizeLineEndings(rawDescribe);

            // use revision lookup to determine if describe is valid - on Windows systems we don't get an error code when doing an invalid
            // lookup, so we need to "probe" the text this way.
            string revision = Find(rawDescribe, @"change ([\d]+) ", RegexOptions.IgnoreCase);
            if (string.IsNullOrEmpty(revision))
                throw new Exception($"P4 describe failed, got invalid content \"{rawDescribe}\".");

            // s modifier selects across multiple lines
            string descriptionRaw = Find(rawDescribe, @"\n\n(.*?)\n\nAffected files ...", RegexOptions.IgnoreCase & RegexOptions.Multiline).Trim();
            IList<ChangeFile> files = new List<ChangeFile>();

            // affected files is large block listing all files which have been affected by revision
            string affectedFilesRaw = string.Empty;
            if (rawDescribe.Contains("Differences ..."))
            {
                affectedFilesRaw = Find(rawDescribe, @"\n\nAffected files ...\n\n(.*)?\n\nDifferences ...", RegexOptions.IgnoreCase);
            }
            else 
            {
                int position = rawDescribe.IndexOf("\n\nAffected files ...");
                if (position > 0)
                    // use substring because regex is too shit to ignore linebreaks
                    affectedFilesRaw = rawDescribe.Substring(position, rawDescribe.Length - position);  // Find(rawDescribe, @"\n\nAffected files ...\n\n(.*)?", RegexOptions.IgnoreCase);
            }
            

            // multiline grab
            string differencesRaw = Find(rawDescribe, @"\n\nDifferences ...\n\n(.*)", RegexOptions.IgnoreCase);

            affectedFilesRaw = affectedFilesRaw == null ? string.Empty : affectedFilesRaw;
            IEnumerable<string> affectedFiles = affectedFilesRaw.Split("\n");

            IEnumerable<string> differences = differencesRaw.Split(@"\n==== ");

            foreach (string affectedFile in affectedFiles)
            {
                Match match = new Regex(@"... (.*)#[\d]+ (delete|add|edit|integrate)$", RegexOptions.IgnoreCase).Match(affectedFile);
                if (!match.Success || match.Groups.Count < 2)
                    continue;

                ChangeFile item = new ChangeFile
                {
                    File = match.Groups[1].Value,
                    Change = match.Groups[2].Value
                };

                // try to get difference
                if (parseDifferences)
                    foreach (string difference in differences)
                    {
                        string file = Find(difference, @" (.*?)#[\d]+ .+ ====");
                        if (file == item.File)
                            item.Differences = Find(difference, @"#.+====(.*)")
                                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
                    }

                files.Add(item);
            }

            IEnumerable<string> description = descriptionRaw.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            description = description.Select(line => line.Trim());

            return new Change
            {
                Revision = revision,
                ChangeFilesCount = affectedFiles.Count(),
                Workspace = Find(rawDescribe, @"change [\d]+ by.+@(.*?) on ", RegexOptions.IgnoreCase),
                Date = DateTime.Parse(Find(rawDescribe, @"change [\d]+ by.+? on (.*?)\n", RegexOptions.IgnoreCase)),
                User = Find(rawDescribe, @"change [\d]+ by (.*?)@", RegexOptions.IgnoreCase),
                Files = files,
                Description = string.Join(" ", description)
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="host"></param>
        /// <param name="filePath"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetRawAnnotate(string username, string password, string host, string trustFingerPrint, string filePath, int? revision = null)
        {
            string ticket = GetTicket(username, password, host, trustFingerPrint);

            string revisionSwitch = string.Empty;
            if (revision.HasValue)
                revisionSwitch = $"@{revision.Value}";

            string command = $"p4 -u {username} -p {host} -P {ticket} annotate -c {filePath}{revisionSwitch}";
            ShellResult result = Run(command);
            if (result.ExitCode != 0)
                throw new Exception($"P4 command {command} exited with code {result.ExitCode} : {result.StdErr}");

            return result.StdOut;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static Annotate ParseAnnotate(IEnumerable<string> lines)
        {
            lines = lines.Where(line => !string.IsNullOrEmpty(line));
            string revision = string.Empty;
            string file = string.Empty;
            AnnotateChange change = AnnotateChange.add;
            IList<AnnotateLine> annotateLines = new List<AnnotateLine>();

            // parse out first line, this contains descriptoin
            if (lines.Count() > 0)
            {
                file = Find(lines.ElementAt(0), @"^(.*?) -");
                revision = Find(lines.ElementAt(0), @" change (.*?) ");
                change = (AnnotateChange)Enum.Parse(typeof(AnnotateChange), Find(lines.ElementAt(0), @" - (.*?) change "));
            }

            if (lines.Count() > 1)
                // start start 2nd item in array
                for (int i = 1; i < lines.Count(); i++)
                {
                    // normally first text on annotate line is "revisionnumber: ", but there can be other console noise at end, we 
                    // search for confirmed rev nr, and if not found, we ignore line
                    string rawRevision = Find(lines.ElementAt(i), @"^(.*?): ");
                    if (string.IsNullOrEmpty(rawRevision))
                        continue;

                    AnnotateLine line = new AnnotateLine();
                    annotateLines.Add(line);
                    line.Revision = rawRevision;
                    line.Text = Find(lines.ElementAt(i), @":(.*)$");
                    line.LineNumber = i;
                }

            return new Annotate
            {
                File = file,
                Change = change,
                Revision = revision,
                Lines = annotateLines
            };
        }

        /// <summary>
        /// Gets raw p4 lookup of revisions from now back in time.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="host"></param>
        /// <param name="max"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetRawChanges(string username, string password, string host, string trustFingerPrint, int max = 0, string path = "//...")
        {
            string ticket = GetTicket(username, password, host, trustFingerPrint);

            string maxModifier = max > 0 ? $"-m {max}" : string.Empty;
            string command = $"p4 -u {username} -p {host} -P {ticket} changes {maxModifier} -l {path}";

            ShellResult result = Run(command);
            if (result.ExitCode != 0)
                throw new Exception($"P4 command {command} exited with code {result.ExitCode} : {string.Join("\n", result.StdErr)}");

            return result.StdOut;
        }

        /// <summary>
        /// Gets raw p4 lookup of revisions between two change nrs.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="host"></param>
        /// <param name="trustFingerPrint"></param>
        /// <param name="startRevision"></param>
        /// <param name="endRevision"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<string> GetRawChangesBetween(string username, string password, string host, string trustFingerPrint, int startRevision, int endRevision, string path = "//...")
        {
            string ticket = GetTicket(username, password, host, trustFingerPrint);
            IList<string> revisions = new List<string>();

            string command = $"p4 -u {username} -p {host} -P {ticket} changes {path}@{startRevision},@{endRevision} ";
            ShellResult result = Run(command);
            // bizarrely, p4 changes returns both stdout and stderr for changes
            if (result.ExitCode != 0 || (result.StdErr.Any() && !result.StdOut.Any()))
                throw new Exception($"P4 command {command} exited with code {result.ExitCode} : {string.Join("\n", result.StdErr)}");

            if (result.StdOut.Any()) 
                foreach (string line in result.StdOut)
                {
                    if (!line.StartsWith("Change "))
                        continue;

                    string revisionFound = Find(line, @"change (\d*?) ", RegexOptions.IgnoreCase);
                    
                    // don't include tail-end revision
                    revisions.Add(revisionFound);
                }

            // remove the range revisions, we want only those between
            revisions.Remove(startRevision.ToString());
            revisions.Remove(endRevision.ToString());

            return revisions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawChanges"></param>
        /// <returns></returns>
        public static IEnumerable<Change> ParseChanges(IEnumerable<string> rawChanges)
        {
            List<Change> changes = new List<Change>();
            Change currentChange = new Change();

            foreach (string changeLine in rawChanges)
            {
                if (changeLine.StartsWith("Change "))
                {
                    currentChange = new Change();
                    changes.Add(currentChange);

                    currentChange.Revision = Find(changeLine, @"change ([\d]+) ", RegexOptions.IgnoreCase);
                    currentChange.User = Find(changeLine, @"change [\d]+ on .+ by (.*)@", RegexOptions.IgnoreCase);
                    currentChange.Workspace = Find(changeLine, @"change [\d]+ on .+ by .+@(.*)", RegexOptions.IgnoreCase);
                    currentChange.Date = DateTime.Parse(Find(changeLine, @"change [\d]+ on (.*?) by ", RegexOptions.IgnoreCase));
                }
                else
                {
                    // remove tab chars, replace them with spaces as they server as spaces in formatted p4 messages.
                    // trim to remove those spaces when added add beginning of commit message, where the \t is effectively used as a newline
                    currentChange.Description += changeLine.Replace(@"\t", " ").Trim();
                }
            }

            return changes;
        }
    }
}
