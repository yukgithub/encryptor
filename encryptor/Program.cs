using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace encryptFile
{
	/// <summary>
	/// Program Class
	/// </summary>
	internal class Program
	{
		/// <summary>
        /// 
        /// </summary>
		private static Dictionary<string, string> argConst = new Dictionary<string, string>
		{
			{ "encrypt", "encrypt" },
			{ "decrypt", "decrypt" }
		};

		private const string keyIVFile = "secret_key";
		private const string keySeparate = "/";

		/// <summary>
		/// Entry Point
		/// </summary>
		/// <param name="args"></param>
		private static void Main(string[] args)
		{

			if (!Program.checkArgs(args))
				return;

			string argBehavior = args[0];
			string argInputFile = args[1];
			string argOutputFIle = args[2];
			byte[] key, IV;

			try
			{
				//Encrypt
				if (argBehavior == argConst["encrypt"])
				{
					//Check before encrypt
					if (!checkBeforeEncrypt(argInputFile, argOutputFIle)) return;
					//Run encrypt
					(key, IV) = runEncrypt(argInputFile, argOutputFIle);
					//Save secret key and IV
					outKeyIV(keyIVFile, key, IV);
				}

				//Decrypt
				if (argBehavior == argConst["decrypt"])
                {
					//Check before decrypt
					if (!checkBeforeEncrypt(argInputFile, argOutputFIle)) return;
					//Read secret key and IV
					(key, IV) = inputKeyIV(keyIVFile);
					//Decrypt data and save data to file
					runDecrypt(argInputFile, argOutputFIle, key, IV);
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.Message);
			}

		}

		/// <summary>
        /// Chech arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
		private static bool checkArgs(string[] args)
		{
			if (args.Length != 3)
            {
				Console.WriteLine("Arguments shoud be [`encrypt` or `decrypt'] [input] [output].");
				return false;
			}

			if (!argConst.ContainsValue(args[0]))
            {
				Console.WriteLine("Error: First argument must be `encrypt` or `decrypt.'");
				return false;
			}

			return true;
		}

		/// <summary>
        /// Check before encrypt
        /// </summary>
        /// <param name="inputFIle"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
		private static bool checkBeforeEncrypt(string inputFIle, string outputFile)
		{
            if (!File.Exists(inputFIle))
            {
				Console.WriteLine("Error: Input file does not exist.");
				return false;
			}

			if (File.Exists(outputFile))
            {
				Console.WriteLine("Error: Please delete output file before this command.");
				return false;
			}

			return true;
		}

		/// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
		private static (byte[], byte[]) runEncrypt(string inputFile, string outputFile)
		{
			byte[] key, IV;

			using (Rijndael rijndael = Rijndael.Create())
			{
				key = rijndael.Key;
				IV = rijndael.IV;

				ICryptoTransform transform = rijndael.CreateEncryptor(key, IV);

				using (FileStream fileStream = new FileStream(outputFile, FileMode.CreateNew))
				{
					using (CryptoStream cryptoStream = new CryptoStream(fileStream, transform, CryptoStreamMode.Write))
					{
						using (BinaryWriter binaryWriter = new BinaryWriter(cryptoStream))
						{
							using (FileStream fileStream2 = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
							{
								//write encrypted file
								byte[] array = new byte[fileStream2.Length];
								fileStream2.Read(array, 0, array.Length);
								binaryWriter.Write(array);
							}
						}
					}
				}
			}
			return (key, IV);
		}

		/// <summary>
        /// Write secret key and IV
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="key"></param>
        /// <param name="IV"></param>
		private static void outKeyIV(string outputFile, byte[] key, byte[] IV)
		{
			using (FileStream fileStream = new FileStream(outputFile, FileMode.Create))
			{
				foreach (byte value in key)
					fileStream.WriteByte(value);

				foreach (byte value2 in IV)
					fileStream.WriteByte(value2);

				fileStream.Close();
			}
		}

		/// <summary>
        /// Check before decrypt
        /// </summary>
        /// <param name="inputFIle"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
		private static bool checkBeforeDecrypt(string inputFIle, string outputFile)
		{

            if (!File.Exists(inputFIle)){
				Console.WriteLine("Error: Input file does not exist.");
				return false;
			}

			if (File.Exists(outputFile))
            {
				Console.WriteLine("Error: Please delete output file before this command.");
				return false;
			}

			if (!File.Exists(keyIVFile))
            {
				Console.WriteLine("Error: Secret key file does not exist.");
				return false;
			}

			return true;

		}

		/// <summary>
        /// Read secret key and IV
        /// </summary>
        /// <param name="inputFIle"></param>
        /// <returns></returns>
		private static (byte[], byte[]) inputKeyIV(string inputFIle)
		{
			byte[] key = new byte[32];
			byte[] IV = new byte[16];
			byte[] content;

			using (FileStream fileStream = new FileStream(inputFIle, FileMode.Open))
			{
				content = new byte[fileStream.Length];
				fileStream.Read(content, 0, content.Length);
			}

			content[0..32].CopyTo(key, 0);
			content[32..48].CopyTo(IV, 0);

			return (key, IV);
		}

		/// <summary>
        /// Decrypt data and save to file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="key"></param>
        /// <param name="IV"></param>
		private static void runDecrypt(string inputFile, string outputFile, byte[] key, byte[] IV)
		{
			using (Rijndael rijndael = Rijndael.Create())
			{
				rijndael.Key = key;
				rijndael.IV = IV;

				ICryptoTransform transform = rijndael.CreateDecryptor(key, IV);

				using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
				{
					using (CryptoStream cryptoStream = new CryptoStream(fileStream, transform, CryptoStreamMode.Read))
					{
						using (BinaryReader binaryReader = new BinaryReader(cryptoStream))
						{
							//CryptStream does not have seek property
							byte[] content = new byte[0];
							try
							{
								while(true)
								{
									Array.Resize(ref content, content.Length + 1);
									content[content.Length - 1] = binaryReader.ReadByte();
								}
							}
							catch (EndOfStreamException ex)
							{
								//delete last element
								Array.Resize(ref content, content.Length - 1);
							}

							//write decrypted data to file
							using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
							{
								foreach (byte value in content)
									binaryWriter.Write(value);
							}

						}
					}
				}
			}
		}

	}
}
