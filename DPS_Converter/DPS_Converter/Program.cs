using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace DPS_Converter
{
    class DPS_Conversion
    {
        static string ProductNum;       // 品番(組織名)
        static ushort PickCount;        // ピック数
        static string[] DobbyPtn;       // ドビー情報
        static string[] ColorInfo;      // カラー情報
        static byte CcInfo;             // CC有無

        static void Main(string[] args)
        {
            // 開始メッセージ
            Console.WriteLine("Start");

            try
            {
                // ファイルから必要な情報を取得
                GetDpsFileInfo(args[0]);

                // 品番から不要なスペース削除
                string ProductName = ProductNum.Replace(" ", "");

                // 変換対象メッセージ
                Console.WriteLine("ProductName:" + ProductName);

                // 実行ファイルの場所に出力フォルダ作成
                string appPath = Assembly.GetExecutingAssembly().Location;
                Directory.SetCurrentDirectory(Path.GetDirectoryName(appPath) + "\\Data" );
                if (!Directory.Exists(ProductName))
                {
                    Directory.CreateDirectory(ProductName);
                }

                // dbyファイルに書き出し
                MakeDbyFile(ProductName);

                // psdファイルに書き出し
                MakePsdFile(ProductName);

                // 終了メッセージ
                Console.WriteLine("→Finish!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Console.WriteLine("Press Any Key");
            //Console.Read();
        }

        // ***DPSファイルから各情報を取得***
        private static void GetDpsFileInfo(string tFilePass)
        {
            using (BinaryReader w = new BinaryReader(File.OpenRead(tFilePass)))
            {
                try
                {
                    w.ReadBytes(3);                                     // 読み飛ばし
                    string Pic = new string(w.ReadChars(4));            // ピック数
                    PickCount = Convert.ToUInt16(Pic,16);
                    w.ReadBytes(2);                                     // 読み飛ばし
                    ProductNum = new string(w.ReadChars(12));           // 品番(組織名)
                    w.ReadBytes(6);                                     // 読み飛ばし
                    CcInfo = w.ReadByte();                              // CC有無
                    w.ReadBytes(30);                                    // 読み飛ばし
                    DobbyPtn = new string[PickCount];                   // ピック分のサイズを準備
                    ColorInfo = new string[PickCount];                  // 同じく
                    // ピック毎に読み込む
                    for (int i = 0; i < PickCount; i++)
                    {
                        w.ReadBytes(11);                            // 読み飛ばし
                        DobbyPtn[i] = new string(w.ReadChars(4));   // ドビーパターン
                        w.ReadByte();                               // 読み飛ばし
                        ColorInfo[i] = new string(w.ReadChars(1));  // カラーパターン
                        w.ReadBytes(6);                             // 読み飛ばし
                    }
                }
                catch (EndOfStreamException)
                {
                }
                w.Close();
            }
            return;
        }

        // ***dbyファイル作成***
        private static void MakeDbyFile( string sFldName )
        {
            // バイナリ形式でdbyファイルに書き出し
            string sDbyFile = sFldName + ".dby";
            using (StreamWriter w = new StreamWriter(File.OpenWrite(@sFldName + "\\" + sDbyFile)))
            {
                w.Write("patfil\0\0\0");
                w.Write( ProductNum.Substring(0, 8));
                w.Write( ConvertHexToString( PickCount )); 
                for (int i = 0; i < PickCount; i++)
                    w.Write("\0" + ConvertDpsToDby((DobbyPtn[i])) + "0000" + ColorInfo[i] + "0" );
                w.Write("\0");
                w.Close();
            }
            return;
        }

        // ***psdファイル作成***
        private static void MakePsdFile( string sFldName)
        {
            // バイナリ形式でpsdファイルに書き出し
            string sPsdFile = sFldName + ".psd";
            using (StreamWriter w = new StreamWriter(File.OpenWrite(@sFldName + "\\" + sPsdFile)))
            {
                w.Write("SD Card R/W                                 00000000000000000000000000000001        ");
                w.Write( ProductNum.Substring(0,8));
                for (int i = 0; i < 5232; i++)
                    w.Write("0");
                w.Close();
            }
            return;
        }

        // ***16進数表記文字列のバイトオーダー反転***
        private static string ConvertDpsToDby(string byteString)
        {
           StringBuilder sb = new StringBuilder();
            // 文字列の1文字目と2文字、3文字目と4文字目を反転
            sb.Append(byteString.Substring(1,1));
            sb.Append(byteString.Substring(0,1));
            sb.Append(byteString.Substring(3,1));
            sb.Append(byteString.Substring(2,1));
            
            return sb.ToString();
        }

        // ***16進数表記文字列のバイトオーダー反転***
        private static string ConvertHexToString(ushort usHexData)
        {
            // byte配列に置き換えてstringへ
            byte[] bytes = BitConverter.GetBytes(usHexData);
            string byteString = BitConverter.ToString(bytes).Replace("-", "");

            StringBuilder sb = new StringBuilder();

            // 0x10未満
            if (usHexData < 0x10)
            {
                sb.Append("   ");
                sb.Append(byteString.Substring(1, 1));
            }
            // 0x100未満
            else if(usHexData < 0x100)
            {
                sb.Append("  ");
                sb.Append(byteString.Substring(0, 2));
            }
            // 0x1000未満
            else if(usHexData < 0x1000)
            {
                sb.Append(" ");
                sb.Append(byteString.Substring(3, 1));
                sb.Append(byteString.Substring(0, 2));
            }
            // 0x1000以上
            else
            {
                sb.Append(byteString.Substring(2, 2));
                sb.Append(byteString.Substring(0, 2));
            }
            return sb.ToString();
        }
    }
}
