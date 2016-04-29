using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Microsoft.Build.Framework;
using XmlPrime.Contracts;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// 	The base class of the <see cref = "Query" /> and <see cref = "Transform" /> tasks.
    /// </summary>
    // TODO: Add extensibility points to allow derived tasks to customize static and dynamic settings.
    public abstract class XmlPrimeTask : XmlPrimeSerializationTask
    {
        #region Private Constants

        private const string DefaultCollection = "default:///";

        #endregion

        #region Private Methods

        private ValidationType GetValidationType()
        {
            if (string.IsNullOrEmpty(Validation) ||
                string.Equals(Validation, "none", StringComparison.InvariantCultureIgnoreCase))
                return ValidationType.None;
            if (string.Equals(Validation, "dtd", StringComparison.InvariantCultureIgnoreCase))
                return ValidationType.DTD;
            if (string.Equals(Validation, "schema", StringComparison.InvariantCultureIgnoreCase))
                return ValidationType.Schema;

            Log.LogWarning("Validation type {0} not recognised.  Should be 'dtd', 'schema' or 'none'.", Validation);
            return ValidationType.None;
        }

        private void LogXdmException(Severity severity, [CanBeNull] Exception innerException)
        {
            //TODO: Only the outermost exception is an error, the rest should be reported as information level log items.
            while (innerException != null)
            {
                var ex = innerException as XdmException;
                if (ex != null)
                {
                    var source = ex.SourceUri == null
                                     ? null
                                     : new Uri(ex.SourceUri);
                    //TODO: specify the subcategory, if appropriate
                    if (severity == Severity.Error)
                    {
                        Log.LogError(null,
                                     ex.ErrorCode.Name,
                                     ex.HelpLink,
                                     source == null
                                         ? null
                                         : source.Scheme == Uri.UriSchemeFile
                                               ? source.LocalPath
                                               : source.ToString(),
                                     ex.LineNumber,
                                     ex.LinePosition,
                                     ex.LineNumber,
                                     ex.LinePosition,
                                     ex.Message);
                    }
                    else
                    {
                        Log.LogWarning(null,
                                       ex.ErrorCode.Name,
                                       ex.HelpLink,
                                       source == null
                                           ? null
                                           : source.Scheme == Uri.UriSchemeFile
                                                 ? source.LocalPath
                                                 : source.ToString(),
                                       ex.LineNumber,
                                       ex.LinePosition,
                                       ex.LineNumber,
                                       ex.LinePosition,
                                       ex.Message);
                    }
                }
                else if (severity == Severity.Error)
                    Log.LogErrorFromException(innerException);
                else
                    Log.LogWarningFromException(innerException);

                innerException = innerException.InnerException;
            }
        }

        private void LogXmlException(Severity severity, [CanBeNull] Exception innerException)
        {
            //TODO: Only the outermost exception is an error, the rest should be reported as information level log items.
            while (innerException != null)
            {
                var ex = innerException as XmlException;
                if (ex != null)
                {
                    var source = ex.SourceUri == null
                                     ? null
                                     : new Uri(ex.SourceUri);
                    //TODO: specify the subcategory, if appropriate
                    if (severity == Severity.Error)
                    {
                        Log.LogError(null,
                                     string.Empty,
                                     ex.HelpLink,
                                     source == null
                                         ? null
                                         : source.Scheme == Uri.UriSchemeFile
                                               ? source.LocalPath
                                               : source.ToString(),
                                     ex.LineNumber,
                                     ex.LinePosition,
                                     ex.LineNumber,
                                     ex.LinePosition,
                                     ex.Message);
                    }
                    else
                    {
                        Log.LogWarning(null,
                                       string.Empty,
                                       ex.HelpLink,
                                       source == null
                                           ? null
                                           : source.Scheme == Uri.UriSchemeFile
                                                 ? source.LocalPath
                                                 : source.ToString(),
                                       ex.LineNumber,
                                       ex.LinePosition,
                                       ex.LineNumber,
                                       ex.LinePosition,
                                       ex.Message);
                    }
                }
                else if (severity == Severity.Error)
                    Log.LogErrorFromException(innerException);
                else
                    Log.LogWarningFromException(innerException);

                innerException = innerException.InnerException;
            }
        }

        private bool TryAddExpressionParameterr([NotNull] DynamicContextSettings dynamicContextSettings,
                                                [NotNull] XmlQualifiedName parameterName,
                                                [NotNull] string select,
                                                [NotNull] XPathSettings settings)
        {
            Assert.ArgumentNotNull(dynamicContextSettings, "dynamicContextSettings");
            Assert.ArgumentNotNull(parameterName, "parameterName");
            Assert.ArgumentNotNull(select, "select");
            Assert.ArgumentNotNull(settings, "settings");

            Log.LogMessage(MessageImportance.Low,
                           "adding parameter '{0}' as expression '{1}'", parameterName,
                           select);

            try
            {
                // TODO: Set base URI, handle errors differently
                var selectExpr = XPath.Compile(select, settings);

                if (selectExpr == null)
                {
                    Log.LogWarning("Parameter '{0}' has invalid value '{1}'", parameterName, select);
                    return false;
                }

                var definition = selectExpr.Evaluate(dynamicContextSettings);
                dynamicContextSettings.Parameters.Add(parameterName, definition);
                return true;
            }
            catch (XdmException e)
            {
                LogXdmException(Severity.Error, e.InnerException);
                return false;
            }
        }

        private bool TryAddPathParameterr([NotNull] DynamicContextSettings dynamicContextSettings,
                                          [NotNull] XmlQualifiedName parameterName,
                                          [NotNull] string path)
        {
            Assert.ArgumentNotNull(dynamicContextSettings, "dynamicContextSettings");
            Assert.ArgumentNotNull(parameterName, "parameterName");
            Assert.ArgumentNotNull(path, "path");

            Log.LogMessage(MessageImportance.Low, "adding parameter '{0}' as document '{1}'", parameterName, path);

            try
            {
                //TODO: use AnyURI when it supports windows paths
                var fileInfo = new FileInfo(path);
                var inputUri = new Uri(fileInfo.FullName);
                var navigable = dynamicContextSettings.DocumentSet.Document(new AnyUri(inputUri));
                dynamicContextSettings.Parameters.Add(parameterName, navigable.CreateNavigator());
                return true;
            }
            catch (XmlException ex)
            {
                Log.LogErrorFromException(ex, false, true, ex.SourceUri);
                return false;
            }
            catch (ArgumentException ex)
            {
                Log.LogErrorFromException(ex, false, true, null);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                Log.LogErrorFromException(ex, false, true, null);
                return false;
            }
        }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// 	Initializes a new instance of the <see cref = "XmlPrimeTask" /> class.
        /// </summary>
        protected XmlPrimeTask()
        {
            // TODO: we should really be using ResourceResolvers rather than XmlResolvers (when we support them).

            OptimizationLevel = 3;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 	Gets the XDM type of the context item.
        /// </summary>
        /// <returns>A <see cref = "XdmType" /> or <see langword = "null" /> if no context item is defined.</returns>
        protected XdmType GetContextItemType()
        {
            if (Input == null)
                return null;
            if (GetValidationType() ==
                ValidationType.Schema)
                return XdmType.Document();

            return XdmType.UntypedDocument();
        }

        /// <summary>
        /// 	Gets the optimization level.
        /// </summary>
        /// <returns>One of the <see cref = "OptimizationLevel" /> values.</returns>
        protected OptimizationLevel GetOptimizationLevel()
        {
            switch (OptimizationLevel)
            {
                case 0:
                    return XmlPrime.OptimizationLevel.None;

                case 1:
                    return XmlPrime.OptimizationLevel.Local;

                case 2:
                    return XmlPrime.OptimizationLevel.Global;

                case 4:
                    return XmlPrime.OptimizationLevel.Specialization;

                default:
                    return XmlPrime.OptimizationLevel.Join;
            }
        }

        /// <summary>
        /// 	Gets the XML version.
        /// </summary>
        /// <returns>One of the <see cref = "XmlVersion" /> values.</returns>
        protected XmlVersion GetXmlVersion()
        {
            return XmlVersion.Xml10;
        }

        /// <summary>
        /// 	Loads the collections.
        /// </summary>
        /// <param name = "dynamicContextSettings">The dynamic context settings.</param>
        /// <param name = "staticSettings">The static settings.</param>
        protected void LoadCollections([NotNull] DynamicContextSettings dynamicContextSettings,
                                       [NotNull] StaticContextSettings staticSettings)
        {
            Assert.ArgumentNotNull(dynamicContextSettings, "dynamicContextSettings");
            Assert.ArgumentNotNull(staticSettings, "staticSettings");

            // TODO: We should be specifying collections as a list of URIs, not as a list of files.
            // This way the collection can contain nodes wihtin a document (with fragment identifiers).
            // We are going to have to move this way anyway to handle uri-collection

            if (Collections == null)
                return;

            var collections = new Dictionary<AnyUri, List<IXPathNavigable>>();
            foreach (var item in Collections)
            {
                var uriString = item.GetMetadata("CollectionURI");
                Log.LogMessage(MessageImportance.Low, "adding " + item.ItemSpec + " to collection " + uriString);

                //TODO: use AnyURI when it supports windows paths
                var itemUri = new Uri(item.GetMetadata("FullPath"));
                var navigable = dynamicContextSettings.DocumentSet.Document(new AnyUri(itemUri));

                if (string.IsNullOrEmpty(uriString))
                    uriString = DefaultCollection;
                List<IXPathNavigable> list;
                var uri = new AnyUri(uriString);
                if (!collections.TryGetValue(uri, out list))
                {
                    list = new List<IXPathNavigable>();
                    collections[uri] = list;
                }
                list.Add(navigable);
            }

            foreach (var collection in collections)
                dynamicContextSettings.DocumentSet.AddCollection(collection.Key, collection.Value.ToArray());
            dynamicContextSettings.DefaultCollectionURI = new AnyUri(DefaultCollection);
            staticSettings.CollectionTypeResolver = dynamicContextSettings.DocumentSet.CollectionTypeResolver;
        }

        /// <summary>
        /// Loads the context item.
        /// </summary>
        /// <param name="dynamicContextSettings">The dynamic context settings.</param>
        /// <returns><see langword="true" /> if the loading the context item succeeded; otherwise <see langword="false" />.</returns>
        protected bool LoadContextItem([NotNull] DynamicContextSettings dynamicContextSettings)
        {
            Assert.ArgumentNotNull(dynamicContextSettings, "dynamicContextSettings");

            if (Input == null)
                return true;

            var item = LoadContextItem(dynamicContextSettings.DocumentSet, Input);

            if (item == null)
                return false;

            dynamicContextSettings.ContextItem = item.CreateNavigator();

            return true;
        }

        /// <summary>
        /// 	Event handler method for logging compilation errors and warnings, <c>fn:trace</c> and <c>xsl:message</c>
        /// </summary>
        /// <param name = "sender">The sender.</param>
        /// <param name = "e">The <see cref = "XmlPrime.CompilationErrorEventArgs" /> instance containing the event data.</param>
        protected void OnCompilationError(object sender, [NotNull] CompilationErrorEventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            Exception innerException = e.CompilationError;

            LogXdmException(e.Severity, innerException);
        }

        /// <summary>
        /// 	Event handler method for logging compilation errors and warnings, <c>fn:trace</c> and <c>xsl:message</c>
        /// </summary>
        /// <param name = "sender">The sender.</param>
        /// <param name = "e">The <see cref = "XmlPrime.TraceEventArgs" /> instance containing the event data.</param>
        protected void OnTrace(object sender, [NotNull] TraceEventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            var source = e.SourceUri == null
                             ? null
                             : new Uri(e.SourceUri);
            var msg = string.Format("{0}: {1}, {2}: {3}: {4}",
                                    source == null
                                        ? null
                                        : source.Scheme == Uri.UriSchemeFile
                                              ? source.LocalPath
                                              : source.ToString(),
                                    e.LineNumber,
                                    e.LinePosition,
                                    e.Label,
                                    e.Value);
            Log.LogMessage(MessageImportance.Normal, msg);
        }

        /// <summary>
        /// 	Sets the parameters from the <see cref = "Parameters" /> property.
        /// </summary>
        /// <param name = "dynamicContextSettings">The dynamic context settings.</param>
        /// <param name = "staticContextSettings">The static context settings.</param>
        /// <returns><see langword="true" /> if settings the parameters succeeded; otherwise, <see langword="false" />.</returns>
        protected bool SetParameters([NotNull] DynamicContextSettings dynamicContextSettings,
                                     [NotNull] StaticContextSettings staticContextSettings)
        {
            Assert.ArgumentNotNull(dynamicContextSettings, "dynamicContextSettings");
            Assert.ArgumentNotNull(staticContextSettings, "staticContextSettings");

            if (string.IsNullOrEmpty(Parameters))
                return true;

            var settings = new XPathSettings(staticContextSettings.NameTable)
                               {
                                   BaseURI = staticContextSettings.BaseURI,
                                   CollationResolver = staticContextSettings.CollationResolver,
                                   CollectionTypeResolver = staticContextSettings.CollectionTypeResolver,
                                   DefaultCollation = staticContextSettings.DefaultCollation,
                                   DocumentTypeResolver = staticContextSettings.DocumentTypeResolver,
                                   DefaultFunctionNamespace = staticContextSettings.DefaultFunctionNamespace,
                                   Schemas = staticContextSettings.Schemas,
                                   XmlVersion = staticContextSettings.XmlVersion,
                                   Libraries = staticContextSettings.Libraries
                               };

            var xmlReaderSettings = new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment};
            try
            {
                using (var reader = XmlReader.Create(new StringReader(Parameters), xmlReaderSettings))
                {
                    reader.Read();
                    while (!reader.EOF)
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                            {
                                if (reader.LocalName != "Parameter")
                                    continue;

                                var name = reader.GetAttribute("Name");
                                var namespaceUri = reader.GetAttribute("Namespace") ?? string.Empty;

                                if (name == null)
                                {
                                    Log.LogWarning(
                                        "Invalid parameter declaration: {0}\nParameter must contain a Name attribute",
                                        reader.ReadOuterXml());
                                    continue;
                                }

                                var value = reader.GetAttribute("Value");
                                var select = reader.GetAttribute("Select");
                                var path = reader.GetAttribute("Path");

                                if (value == null &&
                                    select == null &&
                                    path == null)
                                {
                                    Log.LogWarning(
                                        "Invalid parameter declaration: {0}\nParameter must contain either a Value, Select or Path attribute",
                                        reader.ReadOuterXml());
                                    continue;
                                }

                                if (value != null &&
                                    (select != null || path != null) ||
                                    (select != null && path != null))
                                {
                                    Log.LogWarning(
                                        "Invalid parameter declaration: {0}\nParameter must contain either a Value, Select or Path attribute",
                                        reader.ReadOuterXml());
                                    continue;
                                }

                                var parameterName = new XmlQualifiedName(name, namespaceUri);

                                if (!dynamicContextSettings.Parameters.Contains(parameterName))
                                {
                                    if (value != null)
                                    {
                                        Log.LogMessage(MessageImportance.Low, "adding parameter '{0}' as value '{1}'",
                                                       parameterName, value);

                                        dynamicContextSettings.Parameters.AddUntypedAtomic(parameterName, value);
                                    }
                                    else if (path != null)
                                    {
                                        if (TryAddPathParameterr(dynamicContextSettings,
                                                                 parameterName,
                                                                 path) == false)
                                            return false;
                                    }
                                    else
                                    {
                                        settings.Namespaces = (IXmlNamespaceResolver) reader;

                                        if (TryAddExpressionParameterr(dynamicContextSettings,
                                                                       parameterName,
                                                                       select,
                                                                       settings) == false)
                                            return false;
                                    }
                                }
                                else
                                    Log.LogWarning("Parameter '{0}' has multiple definitions", parameterName);

                                reader.Skip();
                                break;
                            }

                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                return true;
            }
            catch (XmlException e)
            {
                LogXmlException(Severity.Error, e);
                return false;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" />[] parameter.</para>
        /// 	<para>>Specifies the available collections.</para>
        /// </summary>
        /// <value>
        /// An array of <see cref="ITaskItem" /> instances representing the set of available collections.
        /// </value>
        /// <remarks>
        /// 	If the CollectionURI metadata is defined the item will be added to the specified collection.
        /// 	Otherwise the items will be set as the default collection.
        /// 	If an input file is gzipped (.gz) it will be automatically decompressed before being loaded.
        /// </remarks>
        [CanBeNull]
        public ITaskItem[] Collections { get; set; }

        /// <summary>
        /// <para>Optional <see cref="bool"/> parameter.</para>
        /// <para>Specifies whether the byte code generation is enabled.</para>
        /// </summary>
        /// <value>
        /// <see langword="true" /> to enable byte code compilation; otherwise <see langword="false" />.
        /// </value>
        /// <remarks>
        /// By default, byte code generation is disabled.
        /// </remarks>
        public bool EnableByteCode { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies the XML input file.</para>
        /// </summary>
        /// <value>
        /// A <see cref="ITaskItem" /> representing the input file.
        /// </value>
        /// <remarks>
        /// 	If the input file is gzipped (.gz) it will be automatically decompressed before being loaded.
        /// </remarks>
        [CanBeNull]
        public ITaskItem Input { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "int" /> parameter.</para>
        /// 	<para>Specifies the compilation optimization level.</para>
        /// </summary>
        /// <value>
        /// An <see cref="int" /. value specifying the optimization level.
        /// </value>
        /// <remarks>
        /// 	By default, the optimization level is 3.
        /// </remarks>
        public int OptimizationLevel { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "string" /> parameter.</para>
        /// 	<para>Specifies the values of external parameters.</para>
        /// </summary>
        /// <value>A <see cref="string" /. value specifying the input parameters.</value>
        /// <remarks>
        /// 	Parameters are passed as XML Parameter tags with a <c>Name</c> attribute, an optional <c>NamespaceURI</c> attribute
        ///		and one of <c>Value</c>, <c>Select</c> and <c>Path</c> attributes.
        /// </remarks>
        [NotNull]
        public string Parameters { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies the file to which the query plan is written.</para>
        /// </summary>
        /// <value>A <see cref="ITaskItem" /> instance representing the file to which the query plan is written.</value>
        [CanBeNull]
        public ITaskItem Plan { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 	When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// 	Returns <see langword="true" /> if the task successfully executed; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                return Process();
            }
            catch (XmlException ex)
            {
                LogXmlException(Severity.Error, ex);
                return false;
            }
            catch (XdmException ex)
            {
                LogXdmException(Severity.Error, ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, false, true, null);
                return false;
            }
        }

        #endregion
    }
}