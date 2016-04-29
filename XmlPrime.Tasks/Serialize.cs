using System.IO;
using Microsoft.Build.Framework;
using XmlPrime.Contracts;
using XmlPrime.Serialization;

namespace XmlPrime.Tasks
{
    /// <summary>
    /// Serializes an XML input and outputs to a file.
    /// </summary>
    public class Serialize : XmlPrimeSerializationTask
    {
        #region Protected Methods

        /// <summary>
        /// Performs the serialization.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true" /> if the task successfully executed; otherwise, <see langword="false" />.
        /// </returns>
        protected override bool Process()
        {
            if (PopulateSchemaSet() == false)
                return false;

            var documentSet = GetDocumentSet();

            var contextItem = LoadContextItem(documentSet, Input);

            if (contextItem == null)
                return false;

            var serializationSettings = new XdmWriterSettings();
            if (!SetSerializationSettings(serializationSettings))
                return false;

            using (Stream outputStream = File.Create(Output.ItemSpec))
                XdmWriter.Serialize(contextItem.CreateNavigator(), outputStream, serializationSettings);

            OutputFiles = new[] {Output};

            return true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 	<para>Optional <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies the XML input file.</para>
        /// </summary>
        /// <value>
        /// An <see cref="ITaskItem" /> specifying the input file.
        /// </value>
        /// <remarks>
        /// 	If the input file is gzipped (.gz) it will be automatically decompressed before being loaded.
        /// </remarks>
        [Required]
        [NotNull]
        public ITaskItem Input { get; set; }

        /// <summary>
        /// 	<para>Required <see cref = "ITaskItem" /> parameter.</para>
        /// 	<para>Specifies the output file to which the input will be serialized.</para>
        /// </summary>
        /// <value>
        /// An <see cref="ITaskItem" /> specifying the output file.
        /// </value>
        [Required]
        [NotNull]
        public ITaskItem Output { get; set; }

        #endregion
    }
}