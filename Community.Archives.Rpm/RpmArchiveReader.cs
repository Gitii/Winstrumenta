using System.IO.Compression;
using Community.Archives.Core;
using Community.Archives.Cpio;

namespace Community.Archives.Rpm
{
    public class RpmArchiveReader : IArchiveReader
    {
        public async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
            Stream stream,
            params string[] regexMatcher
        )
        {
            var lead = await stream.ReadStruct<RpmLead>().ConfigureAwait(false);
            AssertLeadIsValid(lead);

            await stream.AlignToBoundary(8);
            var signature = await stream.ReadStruct<RpmHeader>().ConfigureAwait(false);
            AssertHeaderIsValid(signature);

            await stream.SkipStruct<RpmHeaderIndex>(signature.nindex).ConfigureAwait(false);
            await stream.Skip(signature.hsize).ConfigureAwait(false);

            await stream.AlignToBoundary(8);
            var header = await stream.ReadStruct<RpmHeader>().ConfigureAwait(false);
            await stream.SkipStruct<RpmHeaderIndex>(header.nindex).ConfigureAwait(false);
            await stream.Skip(header.hsize).ConfigureAwait(false);

            var payload = await stream
                .ReadStream((int)(stream.Length - stream.Position))
                .ConfigureAwait(false);

            var cpio = new CpioArchiveReader();

            var decompressingStream = new GZipStream(payload, CompressionMode.Decompress, false);
            await foreach (var entry in cpio.GetFileEntriesAsync(decompressingStream, regexMatcher))
            {
                yield return entry;
            }
        }

        public async Task<IArchiveReader.ArchiveMetaData> GetMetaData(Stream stream)
        {
            var lead = await stream.ReadStruct<RpmLead>().ConfigureAwait(false);
            AssertLeadIsValid(lead);

            await stream.AlignToBoundary(8);
            var signature = await stream.ReadStruct<RpmHeader>().ConfigureAwait(false);
            AssertHeaderIsValid(signature);

            await stream.SkipStruct<RpmHeaderIndex>(signature.nindex).ConfigureAwait(false);
            await stream.Skip(signature.hsize).ConfigureAwait(false);

            await stream.AlignToBoundary(8);
            var header = await stream.ReadStruct<RpmHeader>().ConfigureAwait(false);
            var headerEntries = await stream
                .ReadStruct<RpmHeaderIndex>(header.nindex)
                .ConfigureAwait(false);
            var headerEntriesData = await stream.ReadStream(header.hsize).ConfigureAwait(false);

            var tag = RpmTagsExtensions.Parse(headerEntries, headerEntriesData.ToArray());

            return new IArchiveReader.ArchiveMetaData()
            {
                Package = lead.GetName(),
                Description = tag.Description ?? String.Empty,
                Version = tag.Version ?? String.Empty,
                Architecture = tag.Architecture ?? String.Empty,
                AllFields = tag.GetFields(),
            };
        }

        public bool SupportsMetaData { get; } = true;

        private void AssertHeaderIsValid(RpmHeader signature)
        {
            if (!signature.IsValid())
            {
                throw new Exception("The header/signature is invalid");
            }
        }

        private void AssertLeadIsValid(in RpmLead lead)
        {
            if (!lead.IsValid())
            {
                throw new Exception("The lead is invalid");
            }
        }
    }
}