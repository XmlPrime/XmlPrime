using System;
using System.IO;
using Microsoft.Build.Framework;
using XmlPrime.Contracts;
using XmlPrime.Serialization;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// Transforms XML inputs using an XSL transformation and output to a file or collection of files.
    /// </summary>
    public class Transform : XmlPrimeTask
    {
        #region Private Methods

        private Xslt CompileTransform([NotNull] XsltSettings staticSettings)
        {
            Assert.ArgumentNotNull(staticSettings, "staticSettings");

            // TODO: use AnyUri when it fully supports windows paths
            var uri = new Uri(Xsl.GetMetadata("FullPath"));
            staticSettings.BaseURI = new AnyUri(uri);
            return Xslt.Compile(uri, staticSettings);
        }

        private void Explain([NotNull] Xslt transform)
        {
            Assert.ArgumentNotNull(transform, "transform");

            Assert.IsNotNull(Plan, "Plan");

            var settings = new XdmWriterSettings
                               {
                                   Indent = true
                               };

            var stream = File.OpenWrite(Plan.GetMetadata("FullPath"));
            using (var xdmWriter = XdmWriter.Create(stream, settings))
                transform.Explain(xdmWriter);

        }

        [NotNull]
        private DynamicContextSettings GetDynamicContextSettings([NotNull] DocumentSet documentSet)
        {
            Assert.ArgumentNotNull(documentSet, "documentSet");

            //TODO: switch to constructing directly when AnyUri fully supports windows paths
            var outputUri = new Uri(Output.GetMetadata("FullPath"));
            var baseOutput = new AnyUri(outputUri);

            var dynamicContextSettings = new DynamicContextSettings
                                             {
                                                 BaseOutputURI = baseOutput,
                                                 DocumentSet = documentSet,
                                                 AvailableEnvironmentVariables =
                                                     EnvironmentVariables.Create()
                                             };

            dynamicContextSettings.Message += OnMessage;
            dynamicContextSettings.Trace += OnTrace;

            return dynamicContextSettings;
        }

        [NotNull]
        private XsltSettings GetStaticContextSettings()
        {
            var staticSettings = new XsltSettings(NameTable)
                                     {
                                         OptimizationLevel = GetOptimizationLevel(),
                                         Schemas = SchemaSet,
                                         ContextItemType = GetContextItemType(),
                                         CompilationWarnings =
                                             CompilationWarnings.DynamicErrors |
                                             CompilationWarnings.ScriptCompilationWarning,
                                         ModuleResolver = XmlResolver,
                                         XmlVersion = GetXmlVersion(),
                                         EnableScript = true,
                                         IsSchemaAware = IsSchemaAware,
                                         CodeGeneration =
                                             EnableByteCode
                                                 ? CodeGeneration.DynamicMethods
                                                 : CodeGeneration.None,
                                         XsltVersion = GetXsltVersion()
                                     };

            staticSettings.CompilationError += OnCompilationError;

            staticSettings.ImportModule(XdmModule.XQuery30Functions);
            staticSettings.ImportModule(XdmModule.XPathMathFunctions);
            staticSettings.ImportModule(XdmModule.ExtensionFunctions);

            var baseUri = new Uri(Path.GetFullPath(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar));
            staticSettings.BaseURI = new AnyUri(baseUri);

            return staticSettings;
        }

        /// <summary>
        /// 	Gets the XSLT version.
        /// </summary>
        /// <returns>One of the <see cref = "XsltVersion" /> values.</returns>
        private XsltVersion GetXsltVersion()
        {
            switch (Version)
            {
                case "3.0":
                    return XsltVersion.Xslt30;
                default:
                    return XsltVersion.Xslt20;
            }
        }

        private void OnMessage(object sender,
                               [NotNull] MessageEventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            var source = e.SourceUri == null
                             ? null
                             : new Uri(e.SourceUri);

            //TODO: Flag to treat messages as warnings
            //TODO: add custom attributes to xsl:message to set the severity.
            var msg = string.Format("{0}: {1}, {2}: {3}",
                                    source == null
                                        ? null
                                        : source.Scheme == Uri.UriSchemeFile
                                              ? source.LocalPath
                                              : source.ToString(),
                                    e.LineNumber,
                                    e.LinePosition,
                                    e.Message);
            Log.LogMessage(MessageImportance.Normal, msg);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Performs the transformation.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true" /> if the task successfully executed; otherwise, <see langword="false" />.
        /// </returns>
        protected override bool Process()
        {
            if (PopulateSchemaSet() == false)
                return false;

            var staticSettings = GetStaticContextSettings();

            var transform = CompileTransform(staticSettings);

            // Any errors will already have been reported.
            if (transform == null)
                return false;

            if (Plan != null)
                Explain(transform);

            transform.SerializationSettings.CloseOutput = true;

            if (!SetSerializationSettings(transform.SerializationSettings))
                return false;

            var documentSet = GetDocumentSet();

            var dynamicContextSettings = GetDynamicContextSettings(documentSet);

            LoadCollections(dynamicContextSettings, staticSettings);

            if (!LoadContextItem(dynamicContextSettings))
                return false;

            if (!SetParameters(dynamicContextSettings, staticSettings))
                return false;

            using (var resultDocument = new ResultDocumentHandler(this,
                                                                  Output,
                                                                  dynamicContextSettings.BaseOutputURI))
            {
                if (InitialTemplate != null)
                {
                    if (InitialMode != null)
                        throw XdmException.BadStylesheetInitiation();

                    var initialTemplateName = ParseQName(InitialTemplate);
                    if (initialTemplateName == null) // TODO: log error
                        return false;

                    transform.CallTemplate(initialTemplateName, dynamicContextSettings, resultDocument);
                }
                else if (InitialMode != null)
                {
                    var initialTemplateMode = ParseQName(InitialMode);
                    if (initialTemplateMode == null) // TODO: log error
                        return false;
                    transform.ApplyTemplates(initialTemplateMode, dynamicContextSettings, resultDocument);
                }
                else
                    transform.ApplyTemplates(dynamicContextSettings, resultDocument);

                resultDocument.Complete();
            }

            return true;
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// </summary>
        public Transform()
        {
            IsSchemaAware = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// <para>Optional <see cref="string"/> parameter.</para>
        /// <para>Specifies the initial template mode to apply when executing the transformation.</para>
        /// </summary>
        /// <value>
        /// A <see cref="string" /> specifying the initial mode, or <see langword="null" /> if no initial mode is specified.
        /// </value>
        /// <remarks>This parameter is in Clark notation. "{URI}local" represents the local name "local" in the namespace "URI", or "local" if no namespace is required.</remarks>
        [CanBeNull]
        public string InitialMode { get; set; }

        /// <summary>
        /// <para>Optional <see cref="string"/> parameter.</para>
        /// <para>Specifies the initial template to call when executing the transformation.</para>
        /// </summary>
        /// <value>
        /// A <see cref="string" /> specifying the initial template, or <see langword="null" /> if no initial template is specified.
        /// </value>
        /// <remarks>This parameter is in Clark notation. "{URI}local" represents the local name "local" in the namespace "URI", or "local" if no namespace is required.</remarks>
        [CanBeNull]
        public string InitialTemplate { get; set; }

        /// <summary>
        /// <para>Optional <see cref="bool"/> parameter.</para>
        /// <para>When <see langword="true" />, indicates that the processor should be schema aware.  Otherwise, the processor should be a basic processor.</para>
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the processor behaves as a Schema-Aware XSLT processor.  
        /// <see langword="false" /> if the processor behaves as a Basic XSLT processor.
        /// </value>
        /// <remarks>
        /// A Basic XSLT processor may sometimes be faster than a schema-aware processor.  The default value is <see langword="false" />.
        /// </remarks>
        /// <seealso href="http://www.w3.org/TR/xslt20/#built-in-types"/>
        public bool IsSchemaAware { get; set; }

        /// <summary>
        /// 	<para>Required <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies the output file from executing the transformation.</para>
        /// </summary>
        /// <value>
        /// A <see cref="ITaskItem" /> specifying the output file of the primary result document.
        /// </value>
        [Required]
        [NotNull]
        public ITaskItem Output { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "string" /> parameter.</para>
        /// 	<para>Specifies the version of the XSLT processor.</para>
        /// </summary>
        /// <value>
        /// A <see cref="string" /> specifying the XSLT processor version.
        /// </value>
        /// <remarks>
        /// Specifies the XSLT version to which the processor should conform.  Accepted values are
        /// "2.0" and "3.0".  If not specified, or not one of the accepted values, the default version is XSLT 2.0.
        /// Note that support for XSLT 3.0 is not complete.
        /// </remarks>
        [CanBeNull]
        public string Version { get; set; }

        /// <summary>
        /// <para>Required <see cref="ITaskItem"/> parameter.</para>
        /// <para>Specifies the XSL transformation file.</para>
        /// </summary>
        /// <value>
        /// An <see cref="ITaskItem" /> specifying the query XSL transformation file.
        /// </value>
        /// <remarks>
        /// This parameter can be given any number of custom metadata items,
        /// which will be used to set parameters for the transformation.
        /// These parameters are in Clark notation. "{uri}local" represents the local name "local" in the namespace "uri", or "local" if no namespace is required.
        /// </remarks>
        [Required]
        [NotNull]
        public ITaskItem Xsl { get; set; }

        #endregion
    }
}