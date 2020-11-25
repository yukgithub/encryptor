using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace encryptor
{
	/// <summary>
	/// encryptor Class
	/// </summary>
	internal class Encryptor
	{
		/// <summary>
        /// First arguments
        /// </summary>
		private static readonly Dictionary<string, string> argConst = new Dictionary<string, string>
		{
			{ "encrypto", "encrypto" },
			{ "decrypt", "decrypt" }
		};

		private const string keyIVFile = "secret_key";
		private const int keyLength = 256;

		/// <summary>
		/// Entry Point
		/// </summary>
		/// <param name="args"></param>
		private static void Main(string[] args)
		{
			//Check arguments
			if (!CheckArgs(args)) return;

			string argBehavior = args[0];
			string argInputFile = args[1];
			string argOutputFIle = args[2];
			byte[] key, IV;

			try
			{
				//Encrypto
				if (argBehavior == argConst["encrypto"])
				{
					//Check before encrypto
					if (!CheckBeforeEncrypto(argInputFile, argOutputFIle)) return;
					//Run encrypto
					(key, IV) = WriteEncryptoFile(argInputFile, argOutputFIle);
					//Save secret key and IV
					WriteKeyIV(keyIVFile, key, IV);
				}

				//Decrypt
				if (argBehavior == argConst["decrypt"])
                {
					//Check before decrypt
					if (!CheckBeforeDecrypt(argInputFile, argOutputFIle)) return;
					//Read secret key and IV
					(key, IV) = ReadKeyIV(keyIVFile);
					//Decrypt data and save data to file
					WriteDecryptFile(argInputFile, argOutputFIle, key, IV);
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
		private static bool CheckArgs(string[] args)
		{
			if (args.Length != 3)
            {
				Console.WriteLine("Arguments shoud be [`encrypto` or `decrypt'] [input] [output].");
				return false;
			}

			if (!argConst.ContainsValue(args[0]))
            {
				Console.WriteLine("Error: First argument must be `encrypto` or `decrypt.'");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Check before encrypto
		/// </summary>
		/// <param name="inputFIle"></param>
		/// <param name="outputFile"></param>
		/// <returns></returns>
		private static bool CheckBeforeEncrypto(string inputFIle, string outputFile)
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
		/// Write encrypted data
		/// </summary>
		/// <param name="inputFile"></param>
		/// <param name="outputFile"></param>
		/// <returns></returns>
		private static (byte[], byte[]) WriteEncryptoFile(string inputFile, string outputFile)
		{
			byte[] key, IV;

			using (Aes aes = Aes.Create())
			{
				aes.KeySize = keyLength;
				key = aes.Key;
				IV = aes.IV;

				ICryptoTransform transform = aes.CreateEncryptor(key, IV);

				using (FileStream outputStream = new FileStream(outputFile, FileMode.CreateNew))
				using (CryptoStream cryptoStream = new CryptoStream(outputStream, transform, CryptoStreamMode.Write))
				using (BinaryWriter binaryWriter = new BinaryWriter(cryptoStream))
				using (FileStream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
				{
					//write encrypted file
					byte[] array = new byte[inputStream.Length];
					inputStream.Read(array, 0, array.Length);
					binaryWriter.Write(array);
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
		private static void WriteKeyIV(string outputFile, byte[] key, byte[] IV)
		{
			//delete secret key file when it already exists
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
				Console.WriteLine($"Warning: A secret key({keyIVFile}) already exists and the file has deleted.");
			}

			using (FileStream fileStream = new FileStream(outputFile, FileMode.Create))
			{

				fileStream.Write(key);
				fileStream.Write(IV);

				fileStream.Close();
			}
		}

		/// <summary>
        /// Check before decrypt
        /// </summary>
        /// <param name="inputFIle"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
		private static bool CheckBeforeDecrypt(string inputFIle, string outputFile)
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
		private static (byte[], byte[]) ReadKeyIV(string inputFIle)
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
		private static void WriteDecryptFile(string inputFile, string outputFile, byte[] key, byte[] IV)
		{
			using (Aes aes = Aes.Create())
			{
				aes.KeySize = keyLength;
				aes.Key = key;
				aes.IV = IV;

				ICryptoTransform transform = aes.CreateDecryptor(key, IV);

				using (FileStream   fileStream   = new FileStream(inputFile, FileMode.Open))
				using (CryptoStream cryptoStream = new CryptoStream(fileStream, transform, CryptoStreamMode.Read))
				using (MemoryStream memory       = new MemoryStream())
				using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(outputFile, FileMode.Create)))
				{

					//Read decrypted data into memory
					cryptoStream.CopyTo(memory);
					byte[] decryptBytes = memory.ToArray();

                    //write decrypted data to file
                    binaryWriter.Write(decryptBytes);

				}
			}
		}

	}
}
