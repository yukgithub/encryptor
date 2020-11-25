# encryptor
A command line tool for AES encryption and decryption.

## Usage
### Encryption
$ dotnet encryptor.dll encrypt [input_file_name] [output_file_name]  

[input_file_name] is target file that will be encrypted.  
After program was executed, a secret key file as generated as 'secret_key.'  

### Decryption
$ dotnet encryptor.dll decrypt [input_file_name] [output_file_name]  

[input_file_name] is encrypted file.  

***********************************

## 使い方
### ファイルを暗号化する
$ dotnet encryptor.dll encrypt [入力ファイル名] [出力ファイル名]  

[入力ファイル名]は暗号化される対象の元ファイルです。
実行後、'secret_key'という秘密鍵のファイルが作成されます。

### ファイルを復号化する
$ dotnet encryptor.dll decrypt [入力ファイル名] [出力ファイル名]  
[入力ファイル名]は暗号化したファイルです。
