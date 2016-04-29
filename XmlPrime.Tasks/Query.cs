using System;
using System.IO;
using Microsoft.Build.Framework;
using XmlPrime.Contracts;
using XmlPrime.Serialization;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// Executes an XQuery with an optional XML input and outputs to a file.
    /// </summary>
    public class Query : XmlPrimeTask
    {
        #region Private Methods

        [CanBeNull]
        private XQuery CompileQuery([NotNull] XQuerySettings staticSettings)
        {
            Assert.ArgumentNotNull(staticSettings, "staticSettings");

            var path = XQuery.GetMetadata("FullPath");
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return XmlPrime.XQuery.Compile(file, staticSettings);
        }

        private void Explain([NotNull] XQuery query)
        {
            Assert.ArgumentNotNull(query, "query");

            Assert.IsNotNull(Plan, "Plan");

            var settings = new XdmWriterSettings
                               {
                                   Indent = true
                               };

            var stream = File.OpenWrite(Plan.GetMetadata("FullPath"));
            using (var xdmWriter = XdmWriter.Create(stream, settings))
                query.Explain(xdmWriter);
        }

        [NotNull]
        private DynamicContextSettings GetDynamicContextSettings([NotNull] DocumentSet documentSet)
        {
            Assert.ArgumentNotNull(documentSet, "documentSet");

            //TODO: switch to constructing directly when AnyUri fully supports windows paths
            var outputPath = Output == null ? BuildEngine.ProjectFileOfTaskNode : Output.GetMetadata("FullPath");
            var outputUri = new Uri(outputPath);
            var baseOutput = new AnyUri(outputUri);

            var dynamicContextSettings = new DynamicContextSettings
                                             {
                                                 BaseOutputURI = baseOutput,
                                                 DocumentSet = documentSet
                                             };

            dynamicContextSettings.Trace += OnTrace;
            return dynamicContextSettings;
        }

        [NotNull]
        private XQuerySettings GetStaticContextSettings()
        {
            var staticSettings = new XQuerySettings(NameTable)
                                     {
                                         OptimizationLevel = GetOptimizationLevel(),
                                         TypeCheckingMode = GetTypeCheckingMode(),
                                         Schemas = SchemaSet,
                                         ContextItemType = GetContextItemType(),
                                         CompilationWarnings =
                                             CompilationWarnings.DynamicErrors | CompilationWarnings.EmptySequence |
                                             CompilationWarnings.ScriptCompilationWarning,
                                         ModuleResolver = XmlResolver,
                                         XmlVersion = GetXmlVersion(),
                                         EnableScript = true,
                                         CodeGeneration =
                                             EnableByteCode ? CodeGeneration.DynamicMethods : CodeGeneration.None,
                                         XQueryVersion = GetXQueryVersion()
                                     };

            staticSettings.CompilationError += OnCompilationError;

            staticSettings.ImportModule(XdmModule.ExtensionFunctions);

            var baseUri = new Uri(Path.GetFullPath(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar));
            staticSettings.BaseURI = new AnyUri(baseUri);

            return staticSettings;
        }

        private TypeCheckingMode GetTypeCheckingMode()
        {
            return EnableStaticTyping
                       ? TypeCheckingMode.Static
                       : TypeCheckingMode.Optimistic;
        }

        /// <summary>
        /// 	Gets the XQuery version.
        /// </summary>
        /// <returns>One of the <see cref = "XQueryVersion" /> values.</returns>
        private XQueryVersion GetXQueryVersion()
        {
            switch (Version)
            {
                case "1.0":
                    return XQueryVersion.XQuery10;
                default:
                    return XQueryVersion.XQuery30;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true" /> if the task successfully executed; otherwise, <see langword="false" />.
        /// </returns>
        protected override bool Process()
        {
            if (PopulateSchemaSet() == false)
                return false;

            var staticSettings = GetStaticContextSettings();

            var query = CompileQuery(staticSettings);

            if (query == null)
                return false;

            if (Plan != null)
                Explain(query);

            var documentSet = GetDocumentSet();

            var dynamicContextSettings = GetDynamicContextSettings(documentSet);

            LoadCollections(dynamicContextSettings, staticSettings);

            if (!LoadContextItem(dynamicContextSettings))
                return false;

            if (!SetParameters(dynamicContextSettings, staticSettings))
                return false;

            if (!SetSerializationSettings(query.SerializationSettings))
                return false;

            using (var resultDocumentHandler = new ResultDocumentHandler(this,
                                                                         Output,
                                                                         dynamicContextSettings.BaseOutputURI))
            {
                query.Serialize(resultDocumentHandler, dynamicContextSettings);

                resultDocumentHandler.Complete();
            }

            return true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// <para>Optional <see cref="bool"/> parameter.</para>
        /// <para>Specifies whether the static typing feature is enabled.</para>
        /// </summary>
        /// <value><see langword="true" /> to enable static typing; otherwise <see langword="false" />.</value>
        /// <remarks>
        /// By default, the static typing feature is disabled.
        /// </remarks>
        public bool EnableStaticTyping { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" /> parameter.  This is required if the query is non-updating.</para>
        /// 	<para>Specifies the output file from executing the query.</para>
        /// </summary>
        /// <value>
        /// A <see cref="ITaskItem" /> specifying the output file, or <see langword="null" /> if the query is updating.
        /// </value>
        [CanBeNull]
        public ITaskItem Output { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "string" /> parameter.</para>
        /// 	<para>Specifies the version of the XQuery processor.</para>
        /// </summary>
        /// <value>
        /// A <see cref="string" /> specifying the XQuery processor version.
        /// </value>
        /// <remarks>
        /// Specifies the XQuery version to which the processor should conform.  Accepted values are
        /// "1.0" and "3.0".  If not specified, or not one of the accepted values, the default version is XQuery 3.0.
        /// </remarks>
        [CanBeNull]
        public string Version { get; set; }

        /// <summary>
        /// <para>Required <see cref="ITaskItem"/> parameter.</para>
        /// <para>Specifies the query file.</para>
        /// </summary>
        /// <value>
        /// An <see cref="ITaskItem" /> specifying the query file.
        /// </value>
        [Required]
        [NotNull]
        public ITaskItem XQuery { get; set; }

        #endregion
    }
}