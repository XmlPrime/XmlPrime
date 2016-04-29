using System.IO;
using Microsoft.Build.Framework;
using XmlPrime.Serialization;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// Performs XInclude processing on an XML input and outputs to a file.
    /// </summary>
    public class Include : XmlPrimeSerializationTask
    {
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

            var includeSettings = new XIncludeSettings
                                      {
                                          DocumentSet = GetDocumentSet(),
                                          FixupXmlBase = BaseUriFixup,
                                          FixupXmlLang = LanguageFixup
                                      };

            var contextItem = LoadContextItem(includeSettings.DocumentSet, Input);

            if (contextItem == null)
                return false;

            var serializationSettings = new XdmWriterSettings();
            if (!SetSerializationSettings(serializationSettings))
                return false;

            using (Stream outputStream = File.Create(Output.ItemSpec))
                XInclude.Process(contextItem.CreateNavigator(), outputStream, serializationSettings, includeSettings);

            OutputFiles = new[] {Output};

            return true;
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Include"/> class.
        /// </summary>
        public Include()
        {
            BaseUriFixup = true;
            LanguageFixup = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 	<para>Optional <see cref = "bool" /> parameter.</para>
        /// 	<para>Specifies whether base URI fixup will be performed.</para>
        /// </summary>
        /// <value>
        /// <see langword="true" /> if URI fixup should be performed; otherwise <see langword="false" />.
        /// </value>
        /// <remarks>
        /// 	By default, base URI fixup will be performed.
        /// </remarks>
        public bool BaseUriFixup { get; set; }

        /// <summary>
        /// 	<para>Required <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies the XML input file.</para>
        /// </summary>
        /// <value>
        /// An <see cref="ITaskItem" /> specifying the input file.
        /// </value>
        /// <remarks>
        /// 	If the input file is gzipped (.gz) it will be automatically decompressed before being loaded.
        /// </remarks>
        [Required]
        public ITaskItem Input { get; set; }

        /// <summary>
        /// 	<para>Optional <see cref = "bool" /> parameter.</para>
        /// 	<para>Specifies whether language fixup will be performed.</para>
        /// </summary>
        /// <value>
        /// <see langword="true" /> if language fixup should be performed; otherwise <see langword="false" />.
        /// </value>
        /// <remarks>
        /// 	By default, language fixup will be performed.
        /// </remarks>
        public bool LanguageFixup { get; set; }

        /// <summary>
        /// 	<para>Required <see cref = "ITaskItem" /> parameter..</para>
        /// 	<para>Specifies the output file to which the processed input is written.</para>
        /// </summary>
        /// <value>
        /// An <see cref="ITaskItem" /> specifying the output file.
        /// </value>
        [Required]
        public ITaskItem Output { get; set; }

        #endregion
    }
}