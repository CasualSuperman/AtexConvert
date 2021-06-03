using System;
using System.IO;
using TeximpNet.DDS;

namespace AtexConvert
{
    public struct TexData { // used for the texture previews
        public byte[] Data;
        public ushort Height;
        public ushort Width;
        public ushort MipLevels;
        public ushort Depth;
        public bool IsReplaced;
        public TextureFormat Format;
    }

    public struct TexReplace {
        public string localPath;
        public int Height;
        public int Width;
        public int Depth;
        public int MipLevels;
        public TextureFormat Format;
    }

    public class TextureManager
    {
        // https://github.com/TexTools/xivModdingFramework/blob/872329d84c7b920fe2ac5e0b824d6ec5b68f4f57/xivModdingFramework/Textures/FileTypes/Tex.cs
        public static bool ImportTexture(string inputLocation, string outputLocation ) {
            TexReplace replaceData;
            bool isDDS = Path.GetExtension( inputLocation ).ToLower() == ".dds";
            if( isDDS ) { // a .dds, use the format that the file is already in
                var ddsFile = DDSFile.Read( inputLocation );
                var format = VFXTexture.DXGItoTextureFormat( ddsFile.Format );
                if( format == TextureFormat.Null )
                    return false;
                using( BinaryWriter writer = new BinaryWriter( File.Open( outputLocation, FileMode.Create ) ) ) {
                    replaceData = CreateAtex( format, ddsFile, writer );
                }
                ddsFile.Dispose();
            } else {
                Console.Error.WriteLine("Only DDS files are supported");
                return false;
            }
            return true;
        }

        // ===== WRITES IMPORTED IMAGE TO LOCAL .ATEX FILE ========
        public static TexReplace CreateAtex(TextureFormat format, DDSContainer dds, BinaryWriter bw, bool convertToA8 = false ) {
            using( MemoryStream ms = new MemoryStream() ) {
                dds.Write( ms );
                using( BinaryReader br = new BinaryReader( ms ) ) {

                    TexReplace replaceData = new TexReplace();
                    replaceData.Format = format;
                    br.BaseStream.Seek( 12, SeekOrigin.Begin );
                    replaceData.Height = br.ReadInt32();
                    replaceData.Width = br.ReadInt32();
                    int pitch = br.ReadInt32();
                    replaceData.Depth = br.ReadInt32();
                    replaceData.MipLevels = br.ReadInt32();

                    bw.Write( IOUtil.MakeTextureInfoHeader( format, replaceData.Width, replaceData.Height, replaceData.MipLevels ).ToArray() );
                    br.BaseStream.Seek( 128, SeekOrigin.Begin );
                    var uncompressedLength = ms.Length - 128;
                    byte[] data = new byte[uncompressedLength];
                    br.Read( data, 0, ( int )uncompressedLength );
                    if( convertToA8 ) { // scuffed way to handle png -> A8. Just load is as BGRA, then only keep the A channel
                        data = VFXTexture.CompressA8( data );
                    }
                    bw.Write( data );

                    return replaceData;
                }
            }
        }
    }
}