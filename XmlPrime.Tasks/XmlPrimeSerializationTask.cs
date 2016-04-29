using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XmlPrime.Contracts;
using XmlPrime.Serialization;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// 	The base class of the <see cref = "Query" /> and <see cref = "Transform" /> tasks.
    /// </summary>
    // TODO: Add extensibility points to allow derived tasks to customize static and dynamic settings.
    public abstract class XmlPrimeSerializationTask : Task
    {
        #region Private Fields

        private readonly XmlNameTable _nameTable;
        private readonly XmlSchemaSet _schemaSet;
        private readonly XmlResolver _xmlResolver;

        #endregion

        #region Private Methods

        private XmlSchemaValidationFlags GetValidationFlags()
        {
            return ProcessXsiSchemaLocation
                       ? XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation
                       : XmlSchemaValidationFlags.None;
        }

        private ValidationType GetValidationType()
        {
            if (string.IsNullOrEmpty(Validation) ||
                string.Equals(Validation, "none", StringComparison.InvariantCultureIgnoreCase))
                return ValidationType.None;
            if (string.Equals(Validation, "dtd", StringComparison.InvariantCultureIgnoreCase))
                return ValidationType.DTD;
            if (string.Equals(Validation, "schema", StringComparison.InvariantCultureIgnoreCase))
                return ValidationType.Schema;

            Log.LogWarning(Resources.UnknownValidationType, Validation);
            return ValidationType.None;
        }

        private void LogXdmException(Severity severity,
                                     [CanBeNull] Exception innerException)
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

        private void LogXmlException(Severity severity,
                                     [CanBeNull] Exception innerException)
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

        private bool TrySetSerializationParameter([NotNull] XdmWriterSettings serializerSettings,
                                                  [NotNull] XmlQualifiedName key,
                                                  [NotNull] string value)
        {
            Assert.ArgumentNotNull(serializerSettings, "serializerSettings");
            Assert.ArgumentNotNull(key, "key");
            Assert.ArgumentNotNull(value, "value");

            try
            {
                if (serializerSettings.SetParameter(key, value))
                {
                    Log.LogMessage(MessageImportance.Low,
                                   Resources.AddingSerializationParameter,
                                   key,
                                   value);
                }
                else
                    Log.LogWarning(Resources.UnrecognisedSerializationParameter, key, value);
            }
            catch (XdmException e)
            {
                LogXdmException(Severity.Error, e.InnerException);
                return false;
            }
            return true;
        }

        #endregion

        #region Protected Static Methods

        /// <summary>
        /// 	Parse a string in Clark notation to create an <see cref = "XmlQualifiedName" />
        /// </summary>
        /// <param name = "name">The qualified name in Clark notation.</param>
        /// <remarks>
        /// 	"{URI}local" represents the local name "local" in the namespace "URI", or "local" if no namespace is required.
        /// </remarks>
        /// <returns>The <see cref = "XmlQualifiedName" /> represented by the <see cref = "string" /> name, or <see langword="null" /> if the name could not be parsed.</returns>
        [CanBeNull]
        protected static XmlQualifiedName ParseQName([NotNull] string name)
        {
            Assert.ArgumentNotNull(name, "name");

            if (name[0] != '{')
                return new XmlQualifiedName(name);

            var endCurly = name.IndexOf('}');
            if (endCurly == -1)
                return null;

            var ns = name.Substring(1, endCurly - 1);
            var localname = name.Substring(endCurly + 1);
            return new XmlQualifiedName(localname, ns);
        }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// 	Initializes a new instance of the <see cref = "XmlPrimeSerializationTask" /> class.
        /// </summary>
        protected XmlPrimeSerializationTask()
        {
            // TODO: we should really be using ResourceResolvers rather than XmlResolvers (when we support them).

            _nameTable = new NameTable();
            var preloaded = new XmlPreloadedResolver(new XmlUrlResolver());
            _xmlResolver = new GZipXmlUrlResolver(preloaded);
            _schemaSet = new XmlSchemaSet(_nameTable) {XmlResolver = _xmlResolver};

            Validation = "none";
            ProcessXsiSchemaLocation = true;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// 	Gets the name table.
        /// </summary>
        /// <value>An <see cref="XmlNameTable" /> instance.</value>
        [NotNull]
        protected XmlNameTable NameTable
        {
            get { return _nameTable; }
        }

        /// <summary>
        /// 	Gets the schema set.
        /// </summary>
        /// <value>An <see cref="XmlSchemaSet" /> Instance.</value>
        [NotNull]
        protected XmlSchemaSet SchemaSet
        {
            get { return _schemaSet; }
        }

        /// <summary>
        /// 	Gets the XML resolver.
        /// </summary>
        /// <value>An <see cref="XmlResolver" /> Instance.</value>
        [NotNull]
        protected XmlResolver XmlResolver
        {
            get { return _xmlResolver; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 	Called by the <see cref = "Execute" /> method to perform the processing.
        /// </summary>
        /// <returns>
        /// 	Returns <see langword = "true" /> if the task successfully executed; otherwise, <see langword = "false" />.
        /// </returns>
        protected abstract bool Process();

        /// <summary>
        /// 	Gets the XML reader settings.
        /// </summary>
        /// <returns>A new <see cref = "XmlReaderSettings" /> instance.</returns>
        [NotNull]
        protected DocumentSet GetDocumentSet()
        {
            var readerSettings = new XmlReaderSettings
                                     {
                                         NameTable = _nameTable,
                                         CloseInput = true,
                                         IgnoreWhitespace = StripWhitespace,
                                         ProhibitDtd = false,
                                         ValidationType = GetValidationType(),
                                         Schemas = _schemaSet,
                                         XmlResolver = _xmlResolver,
                                         ValidationFlags = GetValidationFlags()
                                     };

            var documentSet = new DocumentSet(_xmlResolver, readerSettings, false);
            documentSet.IncludeWellKnownDTDs();
            return documentSet;
        }

        /// <summary>
        /// Loads the context item.
        /// </summary>
        /// <param name="documentSet">The document set.</param>
        /// <param name="input">The input.</param>
        /// <returns>An <see cref="IXPathNavigable" /> instance, or <see langword="null" /> if the context item could not be set.</returns>
        [CanBeNull]
        protected IXPathNavigable LoadContextItem(DocumentSet documentSet, ITaskItem input)
        {
            try
            {
                Log.LogMessage(MessageImportance.Low,
                               Resources.SettingContextItem,
                               input.ItemSpec);

                //TODO: use AnyURI when it supports windows paths
                var inputUri = new Uri(input.GetMetadata("FullPath"));
                var navigable = documentSet.Document(new AnyUri(inputUri));
                return navigable == null
                           ? null
                           : navigable.CreateNavigator();
            }
            catch (XmlException ex)
            {
                Log.LogErrorFromException(ex, false, true, ex.SourceUri);
                return null;
            }
            catch (ArgumentException ex)
            {
                Log.LogErrorFromException(ex, false, true, null);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                Log.LogErrorFromException(ex, false, true, null);
                return null;
            }
        }

        /// <summary>
        /// 	Populates the schema set from the <see cref = "Schemas" /> property.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true" /> if the schema set was successfully populated;  otherwise <see langword="false" />.
        /// </returns>
        protected bool PopulateSchemaSet()
        {
            if (Schemas == null ||
                Schemas.Length == 0)
                return true;

            try
            {
                var schemaReaderSettings = new XmlReaderSettings {XmlResolver = _xmlResolver};
                foreach (var schema in Schemas)
                {
                    using (var reader = XmlReader.Create(schema.ItemSpec, schemaReaderSettings))
                    {
                        var xmlSchema = XmlSchema.Read(reader, null);
                        _schemaSet.Add(xmlSchema);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        /// <summary>
        /// 	Sets the serialization parameters from  from the <see cref = "SerializationParameters" /> property.
        /// </summary>
        /// <param name = "serializerSettings">An <see cref = "XdmWriterSettings" /> instance.</param>
        /// <returns><see langword = "true" /> if setting serialization parameters was successful; otherwise, <see langword = "false" />.</returns>
        protected bool SetSerializationSettings([NotNull] XdmWriterSettings serializerSettings)
        {
            Assert.ArgumentNotNull(serializerSettings, "serializerSettings");

            var xmlReaderSettings = new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Fragment};

            if (SerializationParameterDocument != null)
            {
                Log.LogMessage(MessageImportance.Low,
                               Resources.SettingSerializationParameter,
                               SerializationParameterDocument.ItemSpec);

                var path = SerializationParameterDocument.GetMetadata("FullPath");

                using (var reader = XmlReader.Create(path, xmlReaderSettings))
                    serializerSettings.ParameterDocument(reader);
            }

            if (string.IsNullOrEmpty(SerializationParameters))
                return true;

            try
            {
                using (var reader = XmlReader.Create(new StringReader(SerializationParameters), xmlReaderSettings))
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
                                var value = reader.GetAttribute("Value");
                                var key = new XmlQualifiedName(name, namespaceUri);

                                if (TrySetSerializationParameter(serializerSettings, key, value) == false)
                                    return false;

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
        /// 	<para>Optional <see cref = "ITaskItem" />[] read-only output parameter.</para>
        /// 	<para>Returns the result documents created by the transformation.</para>
        /// </summary>
        /// <value>
        /// An array of <see cref="ITaskItem" /> instances representing the result documents created by the transformation.
        /// </value>
        /// <remarks>
        /// 	MediaType and Encoding custom metadata contain the mime-type and character encoding of each output.
        /// </remarks>
        [Output]
        [CanBeNull]
        public ITaskItem[] OutputFiles { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "bool" /> parameter.</para>
        /// 	<para>Specifies whether <c>xsi:schemaLocation</c> and <c>xsi:noNamespaceSchemaLocation</c> attributes will be processed.</para>
        /// </summary>
        /// <value>
        /// <see langword="true" /> if <c>xs;IschemaLocation</c> and <c>xsi:noNamespaceSchemaLocation</c> attributes will be processed; otherwise <see langword="false" />.
        /// </value>
        /// <remarks>
        /// 	By default, <c>xsi:schemaLocation</c> and <c>xsi:noNamespaceSchemaLocation</c> attributes will be processed.
        /// </remarks>
        public bool ProcessXsiSchemaLocation { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" /> array parameter.</para>
        /// 	<para>Specifies the set of XML Schema files used for validation.</para>
        /// </summary>
        /// <value>
        /// An array of <see cref="ITaskItem" /> instances representing the schemas to be made available for validation.
        /// </value>
        /// <remarks>
        /// 	This parameter is equivalent to the <c>--xsd</c> command line flags.
        /// </remarks>
        [CanBeNull]
        public ITaskItem[] Schemas { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies a serialization parameters document.</para>
        /// </summary>
        /// <value>
        /// A <see cref="ITaskItem" /> specifying a serialization parameters document.
        /// </value>
        /// <remarks>
        /// 	Serialization parameters documents are described in <see href="http://www.w3.org/TR/xslt-xquery-serialization-30/">XSLT and XQuery Serialization 3.0</see>.
        /// </remarks>
        [CanBeNull]
        public ITaskItem SerializationParameterDocument { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "string" /> parameter.</para>
        /// 	<para>Specifies serialization parameters.</para>
        /// </summary>
        /// <value>
        /// A <see cref="string" /> specifying the serialization parameters.
        /// </value>
        /// <remarks>
        /// 	Serialization parameters are passed as XML <c>SerializationParameter</c> tags with <c>Name</c>, <c>NamespaceURI</c> and <c>Value</c> attributes.
        /// </remarks>
        [CanBeNull]
        public string SerializationParameters { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "bool" /> parameter.</para>
        /// 	<para>Specifies whether the whitespace will be stripped from source documents.</para>
        /// </summary>
        /// <value>
        /// <see langword="true" /> if whitespace should be stripped; otherwise <see langword="false" />.
        /// </value>
        /// <remarks>
        /// 	By default, whitespace will be preserved.
        /// </remarks>
        public bool StripWhitespace { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "string" /> parameter.</para>
        /// 	<para>Specifies whether the source documents will be validated using a DTD ("dtd"), XML Schemas ("schema") or no validation will be performed ("none").</para>
        /// </summary>
        /// <value>
        /// A <see cref="string" /> value "dtd", "schema", "none", or <see langword="null" /> for the default behaviour.
        /// </value>
        /// <remarks>
        /// 	By default, no validation is performed.
        /// </remarks>
        [CanBeNull]
        public string Validation { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 	Executes the task.
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