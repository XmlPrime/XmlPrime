using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XmlPrime.Contracts;
using XmlPrime.Serialization;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// Manages the creation of result documents.
    /// </summary>
    internal class ResultDocumentHandler : IResultDocumentHandler
    {
        #region Private Static Methods

        /// <summary>
        /// Gets a temporary file in the same directory as the specified path.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>A new <see cref="FileStream" />.</returns>
        private static FileStream GetTemporaryFile(string localPath)
        {
            var directory = Path.GetDirectoryName(localPath) ?? string.Empty;
            var fileName = Path.GetRandomFileName();
            var tmpPath = Path.Combine(directory, fileName);

            if (directory.Length != 0)
                Directory.CreateDirectory(directory);

            while (true)
            {
                try
                {
                    return new FileStream(tmpPath, FileMode.CreateNew);
                }
                catch (IOException)
                {
                    if (File.Exists(tmpPath) == false)
                        throw;

                    // The file already exists, so try a different name.
                    tmpPath = Path.Combine(directory, Path.GetRandomFileName());
                }
            }
        }

        private static void UndoCreate([NotNull] string filename,
                                       [NotNull] Stream stream)
        {
            Assert.ArgumentNotNull(filename, "filename");
            Assert.ArgumentNotNull(stream, "stream");

            try
            {
                // Ensure that the stream is closed.
                stream.Close();

                // Delete the file.
                File.Delete(filename);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private static void UndoCreate([NotNull] string filename)
        {
            Assert.ArgumentNotNull(filename, "filename");

            try
            {
                // Delete the file.
                File.Delete(filename);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private static Action UndoCreateAction([NotNull] string name,
                                               [NotNull] Stream stream)
        {
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(stream, "stream");

            return () => UndoCreate(name, stream);
        }

        private static void UndoReplace([NotNull] string filename)
        {
            Assert.ArgumentNotNull(filename, "filename");

            var backup = filename + ".bak";
            File.Replace(backup, filename, null);
        }

        #endregion

        #region Private Fields

        private readonly string _baseOutputUri;

        private readonly List<Action> _log = new List<Action>();
        private readonly List<Action> _onComplete = new List<Action>();
        private readonly FileInfo _primaryOutput;
        private readonly List<ITaskItem> _resultDocuments = new List<ITaskItem>();
        private readonly XmlPrimeTask _task;

        #endregion

        #region Private Methods

        private XmlWriter CreateResultDocument([NotNull] XdmWriterSettings settings,
                                               [NotNull] FileInfo fileInfo)
        {
            Assert.ArgumentNotNull(settings, "settings");
            Assert.ArgumentNotNull(fileInfo, "fileInfo");

            _task.Log.LogMessage(MessageImportance.Low, Resources.CreateResultDocument, fileInfo.FullName);
            var taskItem = new TaskItem(fileInfo.FullName);

            var directory = fileInfo.Directory;
            if (directory != null && directory.Exists == false)
                directory.Create();
            var localPath = fileInfo.FullName;
            var stream = GetTemporaryFile(localPath);
            var name = stream.Name;

            taskItem.SetMetadata("MimeType", settings.MediaType ?? string.Empty);
            taskItem.SetMetadata("Encoding", settings.Encoding);

            // Record that, on failure, we must delete the newly created file.
            _log.Add(UndoCreateAction(name, stream));

            // Record that, on completion, we need to move the temporary file to the actual file.
            _onComplete.Add(ReplaceAction(name, localPath, taskItem));

            return XdmWriter.Create(stream, settings);
        }

        private void Replace([NotNull] string sourceFilename,
                             [NotNull] string destinationFilename,
                             [NotNull] ITaskItem taskItem)
        {
            Assert.ArgumentNotNull(sourceFilename, "sourceFilename");
            Assert.ArgumentNotNull(destinationFilename, "destinationFilename");
            Assert.ArgumentNotNull(taskItem, "taskItem");

            if (File.Exists(destinationFilename))
            {
                // Record that, on failure, the destination file should be restored.
                _log.Add(() => UndoReplace(destinationFilename));

                // Backup up the destination file.
                var backup = destinationFilename + ".bak";
                File.Replace(sourceFilename, destinationFilename, backup);
            }
            else
            {
                _log.Add(() => UndoCreate(destinationFilename));

                var directory = Path.GetDirectoryName(destinationFilename);
                if (directory != null && Directory.Exists(directory) == false)
                    Directory.CreateDirectory(directory);

                File.Move(sourceFilename, destinationFilename);
            }

            _resultDocuments.Add(taskItem);
        }

        private Action ReplaceAction([NotNull] string name,
                                     [NotNull] string localPath,
                                     [NotNull] ITaskItem taskItem)
        {
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(localPath, "localPath");
            Assert.ArgumentNotNull(taskItem, "taskItem");

            return () => Replace(name, localPath, taskItem);
        }

        #endregion

        #region Public Constructors

        public ResultDocumentHandler([NotNull] XmlPrimeTask task,
                                     [CanBeNull] ITaskItem output,
                                     [NotNull] AnyUri baseOutputUri)
        {
            Assert.ArgumentNotNull(task, "task");
            Assert.ArgumentNotNull(baseOutputUri, "baseOutputUri");

            _task = task;
            _primaryOutput = output == null
                                 ? null
                                 : new FileInfo(output.ItemSpec);
            _baseOutputUri = baseOutputUri.Normalize();
        }

        #endregion

        #region IResultDocumentHandler Members

        public void Complete()
        {
            foreach (var action in _onComplete)
                action();

            _onComplete.Clear();
            _log.Clear();

            _task.OutputFiles = _resultDocuments.ToArray();
        }

        public void Dispose()
        {
            foreach (var action in _log)
                action();
        }

        public XmlWriter Resolve(string resultDocumentUri,
                                 XdmWriterSettings settings)
        {
            Assert.ArgumentNotNull(resultDocumentUri, "resultDocumentUri");
            Assert.ArgumentNotNull(settings, "settings");

            settings.CloseOutput = true;

            var uri = new Uri(new Uri(_baseOutputUri), resultDocumentUri);
            if (uri.Scheme !=
                Uri.UriSchemeFile)
            {
                _task.Log.LogError(Resources.CannotCreateResultDocument,
                                   resultDocumentUri);
                return null;
            }

            var fileInfo = new AnyUri(resultDocumentUri).Normalize() == _baseOutputUri
                               ? _primaryOutput
                               : new FileInfo(uri.LocalPath);

            if (fileInfo == null)
            {
                _task.Log.LogMessage(MessageImportance.Low,
                                     Resources.DiscardingPrimaryResultDocument,
                                     resultDocumentUri);
                return null;
            }

            return CreateResultDocument(settings, fileInfo);
        }

        #endregion
    }
}